using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Myra.Graphics2D;
using Veldrid;
using Rectangle = System.Drawing.Rectangle;

namespace LibAurora.Graphics;

public sealed class QuadBatch : IDisposable
{
	private const int MaxQuads = 4096;
	private const int MaxVertices = MaxQuads * 4;
	private const int MaxIndices = MaxQuads * 6;

	private readonly IGraphics _graphics;
	private readonly DeviceBuffer _indexBuffer;
	private readonly Pipeline _pipeline;
	private readonly DeviceBuffer _projectionBuffer;
	private readonly ResourceLayout _resourceLayout;
	private readonly DeviceBuffer _vertexBuffer;
	private readonly VertexData[] _vertices = new VertexData[MaxVertices];
	private bool _beginCalled;
	private TextureHandle? _currentTexture;
	private bool _disposed;
	private TextureFiltering _filtering = TextureFiltering.Linear;
	private int _quadCount;
	private Rectangle _scissor;

	public QuadBatch(IGraphics graphics, Shader[] shaders)
	{
		ArgumentNullException.ThrowIfNull(graphics);
		ArgumentNullException.ThrowIfNull(shaders);

		_graphics = graphics;
		var factory = graphics.Factory;

		_resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
			new ResourceLayoutElementDescription("uTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("uSampler", ResourceKind.Sampler, ShaderStages.Fragment),
			new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

		_projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

		var outputDesc = _graphics.Device.SwapchainFramebuffer?.OutputDescription ??
		                 new OutputDescription(null,
			                 new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm_SRgb));

		var vertexLayout = new VertexLayoutDescription(
			new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
			new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
			new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

		var pipelineDesc = new GraphicsPipelineDescription(
			BlendStateDescription.SingleAlphaBlend,
			DepthStencilStateDescription.Disabled,
			new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
			PrimitiveTopology.TriangleList,
			new ShaderSetDescription([vertexLayout], shaders),
			[_resourceLayout],
			outputDesc);

		_pipeline = factory.CreateGraphicsPipeline(ref pipelineDesc);
		_vertexBuffer = factory.CreateBuffer(new BufferDescription(
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

		_indexBuffer = factory.CreateBuffer(new BufferDescription(
			(uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
		graphics.Device.UpdateBuffer(_indexBuffer, 0, indices);
	}

	public Rectangle Scissor
	{
		get => _scissor;
		set
		{
			if (_scissor == value) return;
			Flush();
			_scissor = value;
			ApplyScissor(value);
		}
	}

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		_pipeline.Dispose();
		_projectionBuffer.Dispose();
		_resourceLayout.Dispose();
		_vertexBuffer.Dispose();
		_indexBuffer.Dispose();
	}

	public void Begin(Matrix4x4 projection, TextureFiltering filtering = TextureFiltering.Linear)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		_graphics.CommandList.UpdateBuffer(_projectionBuffer, 0, ref projection);
		_filtering = filtering;
		_currentTexture = null;
		_quadCount = 0;
		_beginCalled = true;
	}

	public void End()
	{
		FlushBatch();
		_beginCalled = false;
		_currentTexture = null;
	}

	public ResourceSet CreateResourceSet(TextureView textureView, TextureFiltering filtering)
	{
		var sampler = filtering switch
		{
			TextureFiltering.Nearest => _graphics.Factory.CreateSampler(SamplerDescription.Point),
			TextureFiltering.Anisotropic => _graphics.Factory.CreateSampler(SamplerDescription.Aniso4x),
			_ => _graphics.Factory.CreateSampler(SamplerDescription.Linear),
		};
		return _graphics.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout, textureView, sampler,
			_projectionBuffer));
	}

	public void DrawQuad(TextureHandle texture,
		Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight,
		Vector2 topLeftUv, Vector2 topRightUv, Vector2 bottomLeftUv, Vector2 bottomRightUv,
		RgbaFloat color)
	{
		DrawQuad(texture,
			new VertexData(topLeft, topLeftUv, color),
			new VertexData(topRight, topRightUv, color),
			new VertexData(bottomLeft, bottomLeftUv, color),
			new VertexData(bottomRight, bottomRightUv, color));
	}

	public void DrawQuad(TextureHandle texture,
		Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight,
		Vector2 topLeftUv, Vector2 topRightUv, Vector2 bottomLeftUv, Vector2 bottomRightUv,
		RgbaFloat topLeftColor, RgbaFloat topRightColor, RgbaFloat bottomLeftColor, RgbaFloat bottomRightColor)
	{
		DrawQuad(texture,
			new VertexData(topLeft, topLeftUv, topLeftColor),
			new VertexData(topRight, topRightUv, topRightColor),
			new VertexData(bottomLeft, bottomLeftUv, bottomLeftColor),
			new VertexData(bottomRight, bottomRightUv, bottomRightColor));
	}

	public void DrawSprite(TextureHandle texture, Vector2 position, Rectangle? source, RgbaFloat color,
		float rotation = 0f, Vector2 scale = default, Vector2 origin = default)
	{
		var textureSize = texture.Size;
		var width = source?.Width ?? textureSize.X;
		var height = source?.Height ?? textureSize.Y;
		if (scale == default) scale = Vector2.One;

		var src = source ?? new Rectangle(0, 0, width, height);
		var invWidth = 1f / textureSize.X;
		var invHeight = 1f / textureSize.Y;
		var uvLeft = src.Left * invWidth;
		var uvTop = src.Top * invHeight;
		var uvRight = src.Right * invWidth;
		var uvBottom = src.Bottom * invHeight;

		var p0 = new Vector2(-origin.X, -origin.Y) * scale;
		var p1 = new Vector2(width - origin.X, -origin.Y) * scale;
		var p2 = new Vector2(-origin.X, height - origin.Y) * scale;
		var p3 = new Vector2(width - origin.X, height - origin.Y) * scale;

		if (rotation != 0f)
		{
			var sin = MathF.Sin(rotation);
			var cos = MathF.Cos(rotation);
			p0 = Rotate(p0, sin, cos);
			p1 = Rotate(p1, sin, cos);
			p2 = Rotate(p2, sin, cos);
			p3 = Rotate(p3, sin, cos);
		}

		DrawQuad(texture,
			position + p0, position + p1, position + p2, position + p3,
			new Vector2(uvLeft, uvTop), new Vector2(uvRight, uvTop),
			new Vector2(uvLeft, uvBottom), new Vector2(uvRight, uvBottom),
			color);
	}

	public void DrawQuad(TextureHandle texture, VertexData topLeft, VertexData topRight, VertexData bottomLeft,
		VertexData bottomRight)
	{
		if (!_beginCalled) throw new InvalidOperationException("QuadBatch.Begin must be called before drawing.");
		if (_quadCount >= MaxQuads) FlushBatch();

		if (_currentTexture.HasValue && _currentTexture.Value.Id != texture.Id) FlushBatch();
		_currentTexture = texture;

		var baseIdx = _quadCount * 4;
		_vertices[baseIdx + 0] = topLeft;
		_vertices[baseIdx + 1] = topRight;
		_vertices[baseIdx + 2] = bottomLeft;
		_vertices[baseIdx + 3] = bottomRight;
		_quadCount++;
	}

	private unsafe void FlushBatch()
	{
		if (_quadCount == 0 || !_currentTexture.HasValue) return;

		var cl = _graphics.CommandList;
		fixed (VertexData* ptr = _vertices)
			cl.UpdateBuffer(_vertexBuffer, 0, (nint)ptr, (uint)(_quadCount * 4 * Unsafe.SizeOf<VertexData>()));

		cl.SetPipeline(_pipeline);
		ApplyScissor(_scissor);
		cl.SetGraphicsResourceSet(0, _currentTexture.Value.ResourceSets[_filtering]);
		cl.SetVertexBuffer(0, _vertexBuffer);
		cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
		cl.DrawIndexed((uint)(_quadCount * 6), 1, 0, 0, 0);

		_quadCount = 0;
	}

	private void Flush()
	{
		if (!_beginCalled) return;

		FlushBatch();
	}

	private void ApplyScissor(Rectangle value)
	{
		if (value == Rectangle.Empty)
		{
			_graphics.CommandList.SetFullScissorRect(0);
			return;
		}

		var framebufferWidth = (int)_graphics.ViewportWidth;
		var framebufferHeight = (int)_graphics.ViewportHeight;
		var left = int.Clamp(value.Left, 0, framebufferWidth);
		var top = int.Clamp(value.Top, 0, framebufferHeight);
		var right = int.Clamp(value.Right, left, framebufferWidth);
		var bottom = int.Clamp(value.Bottom, top, framebufferHeight);

		var width = right - left;
		var height = bottom - top;

		if (width == 0 || height == 0)
		{
			_graphics.CommandList.SetScissorRect(0, 0, 0, 1, 1);
			return;
		}

		_graphics.CommandList.SetScissorRect(0, (uint)left, (uint)top, (uint)width, (uint)height);
	}

	private static Vector2 Rotate(Vector2 value, float sin, float cos) =>
		new(value.X * cos - value.Y * sin, value.X * sin + value.Y * cos);

	[StructLayout(LayoutKind.Sequential)]
	public readonly struct VertexData(Vector2 position, Vector2 texCoord, RgbaFloat color)
	{
		public readonly Vector2 Position = position;
		public readonly Vector2 TexCoord = texCoord;
		public readonly RgbaFloat Color = color;
	}
}