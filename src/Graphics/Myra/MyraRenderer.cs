using System.Numerics;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LibAurora.Core;
using LibAurora.MathUtils;
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
public class MyraRenderer : IMyraRenderer, IRenderer, IFontStashRenderer2
{
	private readonly QuadBatch _batch;
	private readonly IGraphics _graphics;

	private bool _beginCalled;
	private TextureFiltering _filtering = TextureFiltering.Linear;
	private Matrix4x4 _projection;

	/// <summary>Creates a new Myra renderer, allocates GPU buffers, and initializes the rendering pipeline.</summary>
	public MyraRenderer(IGraphics graphics, Shader[] shaders)
	{
		_graphics = graphics;
		_batch = new QuadBatch(graphics, shaders);
		TextureManager = new MyraTextureManager(graphics, this);

		Events.Subscribe<Events.SurfaceResizeEvent>(OnResize);
		OnResize(WindowSize);
	}

	public QuadBatch Batch => _batch;

	/// <summary>Begins a new render batch, resets the quad counter, and sets the active texture filtering mode.</summary>
	public void Begin(TextureFiltering textureFiltering)
	{
		_filtering = textureFiltering;
		_batch.Begin(_projection, textureFiltering);
		_beginCalled = true;
	}
	/// <summary>Flushes the current quad batch to the GPU if any quads are queued and a texture is bound.</summary>
	public void End()
	{
		_batch.End();
		_beginCalled = false;
	}
	public void DrawSprite(object texture, Vector2 pos, Rectangle? src,
		FSColor color, float rotation, Vector2 scale, float depth)
	{
		_batch.DrawSprite((TextureHandle)texture, pos, src, color.ToRgbaFloat(), rotation, scale);
	}

	/// <summary>Queues a textured quad. Automatically flushes on batch overflow or texture change.</summary>
	public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
		ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
	{
		_batch.DrawQuad((TextureHandle)texture,
			topLeft.Position.ToVector2(), topRight.Position.ToVector2(),
			bottomLeft.Position.ToVector2(), bottomRight.Position.ToVector2(),
			topLeft.TextureCoordinate, topRight.TextureCoordinate,
			bottomLeft.TextureCoordinate, bottomRight.TextureCoordinate,
			topLeft.Color.ToRgbaFloat(), topRight.Color.ToRgbaFloat(),
			bottomLeft.Color.ToRgbaFloat(), bottomRight.Color.ToRgbaFloat());
	}
	/// <summary>The texture manager used by FontStashSharp for font atlas textures.</summary>
	public ITexture2DManager TextureManager { get; }

	/// <summary>Always returns <see cref="Myra.Platform.RendererType.Quad"/>.</summary>
	public RendererType RendererType => RendererType.Quad;

	/// <summary>The scissor rectangle for clipping draw output. Setting this updates the GPU scissor rect.</summary>
	public Rectangle Scissor
	{
		get => _batch.Scissor;
		set
		{
			if (_batch.Scissor == value) return;
			_batch.Scissor = value;
		}
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
	public void Begin() => Begin(TextureFiltering.Linear);

	/// <summary>Creates a <see cref="ResourceSet"/> for the given texture view and filtering mode.</summary>
	public ResourceSet CreateResourceSet(TextureView textureView, TextureFiltering filtering) =>
		_batch.CreateResourceSet(textureView, filtering);

	public void Dispose() => _batch.Dispose();

	private void OnResize(Vector2 windowSize)
	{
		_projection = Matrix4x4.CreateOrthographicOffCenter(
			0, windowSize.X,
			windowSize.Y, 0,
			-1, 1);
		if (!_beginCalled) return;
		End();
		Begin(_filtering);
	}
	private void OnResize(Events.SurfaceResizeEvent e) => OnResize(e.Size);
}