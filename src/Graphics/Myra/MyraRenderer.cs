using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LibAurora.Core;
using LibAurora.MathUtils;
using Myra;
using Myra.Graphics2D;
using Myra.Platform;
using Veldrid;
using Rectangle = System.Drawing.Rectangle;
namespace LibAurora.Graphics.Myra;

/// <summary>
/// Veldrid-backed <see cref="IMyraRenderer"/> using Quad mode with dynamic vertex batching.
/// Accumulates up to 4096 quads per batch, flushes on texture change or <see cref="End"/>.
/// Creates and caches <see cref="ResourceSet"/> per texture per filter mode via <see cref="CreateResourceSet"/>.
/// Scissor is reset to full framebuffer on <see cref="Begin"/> and updated by Myra's layout system.
/// </summary>
public class MyraRenderer : IMyraRenderer
{
	private const int MaxQuads = 4096;
	private const int MaxVertices = MaxQuads * 4;
	private const int MaxIndices = MaxQuads * 6;

	private readonly IGraphics _graphics;
	private readonly Pipeline _pipeline;
	private readonly VertexData[] _vertices = new VertexData[MaxVertices];
	private MyraTexture? _currentTexture;
	private TextureFiltering _filtering = TextureFiltering.Linear;
	private DeviceBuffer _indexBuffer;
	private Matrix4x4 _projection;
	private DeviceBuffer _projectionBuffer;

	private int _quadCount;
	private ResourceLayout _resourceLayout;
	private DeviceBuffer _vertexBuffer;

	/// <summary>Creates a new Myra renderer, allocates GPU buffers, and initializes the rendering pipeline.</summary>
	public MyraRenderer(IGraphics graphics, Shader[] shaders)
	{
		_graphics = graphics;
		TextureManager = new MyraTextureManager(graphics, this);

		_resourceLayout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
			new ResourceLayoutElementDescription("uTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("uSampler", ResourceKind.Sampler, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

		_projectionBuffer = Factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

		var fb = _graphics.Device.SwapchainFramebuffer;
		var outputDesc = fb?.OutputDescription ??
		                 new OutputDescription(null,
			                 new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm_SRgb));

		var vertexLayout = new VertexLayoutDescription(
			new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
			new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
			new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

		var pipelineDesc = new GraphicsPipelineDescription(
			BlendStateDescription.SingleAlphaBlend,
			DepthStencilStateDescription.Disabled,
			new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
			PrimitiveTopology.TriangleList,
			new ShaderSetDescription([vertexLayout], shaders),
			[_resourceLayout],
			outputDesc);

		_pipeline = Factory.CreateGraphicsPipeline(ref pipelineDesc);

		_vertexBuffer = Factory.CreateBuffer(new BufferDescription(
			(uint)(MaxVertices * Unsafe.SizeOf<VertexData>()), BufferUsage.VertexBuffer | BufferUsage.Dynamic));

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
		_indexBuffer = Factory.CreateBuffer(new BufferDescription(
			(uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
		GraphicsDevice.UpdateBuffer(_indexBuffer, 0, indices);

		Events.Subscribe<Events.SurfaceResizeEvent>(OnResize);
		OnResize(WindowSize);
	}
	/// <summary>The underlying Veldrid <see cref="Veldrid.GraphicsDevice"/>.</summary>
	public GraphicsDevice GraphicsDevice => _graphics.Device;

	/// <summary>The current command list for recording draw commands.</summary>
	public CommandList CommandList => _graphics.CommandList;

	/// <summary>The resource factory for creating GPU resources.</summary>
	public ResourceFactory Factory => _graphics.Factory;

	/// <summary>Current window/viewport size used for orthographic projection.</summary>
	public Vector2 WindowSize => new(_graphics.ViewportWidth, _graphics.ViewportHeight);

	/// <summary>Begins a new render batch, resets the quad counter, and sets the active texture filtering mode.</summary>
	public void Begin(TextureFiltering textureFiltering)
	{
		_quadCount = 0;
		CommandList.SetFullScissorRect(0);
		_filtering = textureFiltering;
	}
	/// <summary>Flushes the current quad batch to the GPU if any quads are queued and a texture is bound.</summary>
	public unsafe void End()
	{
		if (_quadCount == 0 || !_currentTexture.HasValue) return;

		var cl = _graphics.CommandList;

		fixed (VertexData* ptr = _vertices)
			GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, (nint)ptr, (uint)(_quadCount * 4 * sizeof(VertexData)));

		cl.SetPipeline(_pipeline);
		cl.SetGraphicsResourceSet(0, _currentTexture.Value.ResourceSets[_filtering]);
		cl.SetVertexBuffer(0, _vertexBuffer);
		cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
		cl.DrawIndexed((uint)(_quadCount * 6), 1, 0, 0, 0);

		_quadCount = 0;
	}
	/// <summary>Sprite drawing is not supported in Quad renderer mode.</summary>
	public void DrawSprite(object texture, Vector2 pos, Rectangle? src,
		FSColor color, float rotation, Vector2 scale, float depth)
	{
	}

	/// <summary>Queues a textured quad. Automatically flushes on batch overflow or texture change.</summary>
	public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
		ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
	{
		if (_quadCount >= MaxQuads) End();
		var myraTexture = (MyraTexture)texture;

		if (_currentTexture.HasValue && _currentTexture.Value.Id != myraTexture.Id) End();
		_currentTexture = myraTexture;

		var baseIdx = _quadCount * 4;

		_vertices[baseIdx + 0] = new VertexData
		{
			Position = topLeft.Position.ToVector2(),
			TexCoord = topLeft.TextureCoordinate,
			Color = topLeft.Color.ToRgbaFloat(),
		};
		_vertices[baseIdx + 1] = new VertexData
		{
			Position = topRight.Position.ToVector2(),
			TexCoord = topRight.TextureCoordinate,
			Color = topRight.Color.ToRgbaFloat()
		};
		_vertices[baseIdx + 2] = new VertexData
		{
			Position = bottomLeft.Position.ToVector2(),
			TexCoord = bottomLeft.TextureCoordinate,
			Color = bottomLeft.Color.ToRgbaFloat()
		};
		_vertices[baseIdx + 3] = new VertexData
		{
			Position = bottomRight.Position.ToVector2(),
			TexCoord = bottomRight.TextureCoordinate,
			Color = bottomRight.Color.ToRgbaFloat(),
		};
		_quadCount++;
	}
	/// <summary>The texture manager used by FontStashSharp for font atlas textures.</summary>
	public ITexture2DManager TextureManager { get; }

	/// <summary>Always returns <see cref="Myra.Graphics2D.RendererType.Quad"/>.</summary>
	public RendererType RendererType => RendererType.Quad;

	/// <summary>The scissor rectangle for clipping draw output. Setting this updates the GPU scissor rect.</summary>
	public Rectangle Scissor
	{
		get;
		set
		{
			if (field == value) return;
			//End(); Myra will End by itself before scissor change.
			CommandList.SetScissorRect(0, (uint)value.X, (uint)value.Y, (uint)value.Width, (uint)value.Height);
			field = value;
		}
	}

	/// <summary>Creates a <see cref="ResourceSet"/> for the given texture view and filtering mode.</summary>
	public ResourceSet CreateResourceSet(TextureView textureView, TextureFiltering filtering)
	{
		var sampler = filtering switch
		{
			TextureFiltering.Nearest => Factory.CreateSampler(SamplerDescription.Point),
			TextureFiltering.Anisotropic => Factory.CreateSampler(SamplerDescription.Aniso4x),
			_ => Factory.CreateSampler(SamplerDescription.Linear),
		};
		return Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout, textureView, sampler, _projectionBuffer));
	}
	private void OnResize(Vector2 windowSize)
	{
		_projection = Matrix4x4.CreateOrthographicOffCenter(
			0, windowSize.X,
			windowSize.Y, 0,
			-1, 1);
		GraphicsDevice.UpdateBuffer(_projectionBuffer, 0, ref _projection);
		MyraEnvironment.Reset();
	}
	private void OnResize(Events.SurfaceResizeEvent e) => OnResize(e.Size);

	[StructLayout(LayoutKind.Sequential)]
	private struct VertexData(Vector2 position, Vector2 texCoord, RgbaFloat color)
	{
		public Vector2 Position = position;
		public Vector2 TexCoord = texCoord;
		public RgbaFloat Color = color;
	}
}