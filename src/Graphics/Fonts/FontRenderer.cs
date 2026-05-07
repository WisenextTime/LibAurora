using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FontStashSharp;
using FontStashSharp.Interfaces;
using Veldrid;

namespace LibAurora.Graphics.Fonts;

/// <summary>
/// GPU-based font renderer implementing FontStashSharp's renderer interfaces.
/// Manages vertex/index buffers, pipeline state, and batches draw calls for text rendering.
/// </summary>
public unsafe class FontRenderer : IRenderer, IFontStashRenderer2, IDisposable
{
	private const int MaxQuads = 4096;
	private const int MaxVertices = MaxQuads * 4;
	private const int MaxIndices = MaxQuads * 6;
	private readonly FontAtlas _atlas;

	private readonly IGraphics _graphics;
	private readonly DeviceBuffer _indexBuffer;
	private readonly Pipeline _pipeline;
	private readonly DeviceBuffer _projectionBuffer;
	private readonly ResourceLayout _resourceLayout;
	private readonly ResourceSet _resourceSet;
	private readonly Sampler _sampler;
	private readonly DeviceBuffer _vertexBuffer;
	private readonly FontVertex[] _vertices = new FontVertex[MaxVertices];
	private bool _disposed;
	private int _quadCount;

	/// <summary>
	/// Creates a new font renderer and allocates all required GPU resources.
	/// </summary>
	/// <param name="graphics">The graphics context. Must not be null.</param>
	/// <param name="shaders">The shader array (vertex + fragment) for font rendering.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="graphics"/> or <paramref name="shaders"/> is null.</exception>
	public FontRenderer(IGraphics graphics, Shader[] shaders)
	{
		ArgumentNullException.ThrowIfNull(graphics);
		ArgumentNullException.ThrowIfNull(shaders);

		_graphics = graphics;
		var factory = graphics.Factory;
		var device = graphics.Device;

		_atlas = new FontAtlas(graphics);

		_sampler = factory.CreateSampler(SamplerDescription.Linear);

		_resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
			new ResourceLayoutElementDescription("uFontAtlas", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("uFontSampler", ResourceKind.Sampler, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

		_projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

		_resourceSet =
			factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout, _atlas.View, _sampler, _projectionBuffer));

		var vertexLayout = new VertexLayoutDescription(
			new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
			new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
			new VertexElementDescription("LayerIndex", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
			new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

		var fb = device.SwapchainFramebuffer;
		var outputDesc = fb?.OutputDescription ??
		                 new OutputDescription(null,
			                 new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm_SRgb));

		var pipelineDesc = new GraphicsPipelineDescription(
			BlendStateDescription.SingleAlphaBlend,
			DepthStencilStateDescription.Disabled,
			new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
			PrimitiveTopology.TriangleList,
			new ShaderSetDescription([vertexLayout], shaders),
			[_resourceLayout],
			outputDesc);

		_pipeline = factory.CreateGraphicsPipeline(ref pipelineDesc);

		_vertexBuffer = factory.CreateBuffer(new BufferDescription(
			(uint)(MaxVertices * Unsafe.SizeOf<FontVertex>()), BufferUsage.VertexBuffer | BufferUsage.Dynamic));

		var indices = new ushort[MaxIndices];
		for (ushort i = 0; i < MaxQuads; i++)
		{
			var baseVertex = (ushort)(i * 4);
			var idx = i * 6;
			indices[idx + 0] = baseVertex;
			indices[idx + 1] = (ushort)(baseVertex + 1);
			indices[idx + 2] = (ushort)(baseVertex + 2);
			indices[idx + 3] = (ushort)(baseVertex + 1);
			indices[idx + 4] = (ushort)(baseVertex + 3);
			indices[idx + 5] = (ushort)(baseVertex + 2);
		}

		_indexBuffer = factory.CreateBuffer(new BufferDescription(
			(uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
		device.UpdateBuffer(_indexBuffer, 0, indices);
	}

	/// <summary>Releases all GPU resources allocated by this renderer.</summary>
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		_pipeline.Dispose();
		_vertexBuffer.Dispose();
		_indexBuffer.Dispose();
		_projectionBuffer.Dispose();
		_resourceSet.Dispose();
		_resourceLayout.Dispose();
		_sampler.Dispose();
		_atlas.Dispose();
	}

	/// <summary>The texture atlas manager used by FontStashSharp for glyph storage.</summary>
	public ITexture2DManager TextureManager => _atlas;

	/// <summary>
	/// Draws a single textured quad. Automatically flushes the batch when the maximum
	/// number of quads (<see cref="MaxQuads"/>) is reached.
	/// </summary>
	public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
		ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
	{
		if (_quadCount >= MaxQuads)
			End();

		var h = (PageHandle)texture;
		var layer = (float)h.Layer;
		var baseIdx = _quadCount * 4;

		_vertices[baseIdx + 0] = new FontVertex
		{
			Position = ToVector2(topLeft.Position),
			TexCoord = topLeft.TextureCoordinate,
			LayerIndex = layer,
			Color = ToRgbaFloat(topLeft.Color)
		};
		_vertices[baseIdx + 1] = new FontVertex
		{
			Position = ToVector2(topRight.Position),
			TexCoord = topRight.TextureCoordinate,
			LayerIndex = layer,
			Color = ToRgbaFloat(topRight.Color)
		};
		_vertices[baseIdx + 2] = new FontVertex
		{
			Position = ToVector2(bottomLeft.Position),
			TexCoord = bottomLeft.TextureCoordinate,
			LayerIndex = layer,
			Color = ToRgbaFloat(bottomLeft.Color)
		};
		_vertices[baseIdx + 3] = new FontVertex
		{
			Position = ToVector2(bottomRight.Position),
			TexCoord = bottomRight.TextureCoordinate,
			LayerIndex = layer,
			Color = ToRgbaFloat(bottomRight.Color)
		};

		_quadCount++;
	}

	/// <summary>The underlying Veldrid <see cref="GraphicsDevice"/>.</summary>
	public GraphicsDevice GraphicsDevice => _graphics.Device;

	/// <summary>The current command list for recording draw commands.</summary>
	public CommandList CommandList => _graphics.CommandList;

	/// <summary>The resource factory for creating GPU resources.</summary>
	public ResourceFactory Factory => _graphics.Factory;

	/// <summary>Current window/viewport size used for orthographic projection.</summary>
	public Vector2 WindowSize => new(_graphics.ViewportWidth, _graphics.ViewportHeight);

	/// <summary>Resets the quad counter at the start of a new batch.</summary>
	public void Begin()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		_quadCount = 0;
	}

	/// <summary>
	/// Uploads all pending vertex data to the GPU, sets pipeline state, and issues
	/// an indexed draw call for the current batch. Does nothing if no quads are queued.
	/// </summary>
	public void End()
	{
		if (_quadCount == 0) return;

		var cl = _graphics.CommandList;
		var device = _graphics.Device;

		var projection = Matrix4x4.CreateOrthographicOffCenter(
			0, _graphics.ViewportWidth,
			_graphics.ViewportHeight, 0,
			-1, 1);

		device.UpdateBuffer(_projectionBuffer, 0, ref projection);

		fixed (FontVertex* ptr = _vertices)
			device.UpdateBuffer(_vertexBuffer, 0, (nint)ptr, (uint)(_quadCount * 4 * sizeof(FontVertex)));

		cl.SetPipeline(_pipeline);
		cl.SetGraphicsResourceSet(0, _resourceSet);
		cl.SetVertexBuffer(0, _vertexBuffer);
		cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
		cl.DrawIndexed((uint)(_quadCount * 6), 1, 0, 0, 0);

		_quadCount = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)] private static Vector2 ToVector2(Vector3 v) => new(v.X, v.Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static RgbaFloat ToRgbaFloat(FSColor c) => new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

	[StructLayout(LayoutKind.Sequential)]
	private struct FontVertex
	{
		public Vector2 Position;
		public Vector2 TexCoord;
		public float LayerIndex;
		public RgbaFloat Color;
	}
}