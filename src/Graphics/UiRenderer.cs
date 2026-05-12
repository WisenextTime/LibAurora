using System.Numerics;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LibAurora.Core;
using LibAurora.MathUtils;
using Myra.Graphics2D;
using Myra.Platform;
using Veldrid;
using Rectangle = System.Drawing.Rectangle;

namespace LibAurora.Graphics;

/// <summary>Veldrid-backed renderer shared by Myra UI and FontStashSharp.</summary>
public class UiRenderer : IMyraRenderer, IRenderer, IFontStashRenderer2
{
	private readonly QuadBatch _batch;
	private readonly IGraphics _graphics;

	private bool _beginCalled;
	private TextureFiltering _filtering = TextureFiltering.Linear;
	private Matrix4x4 _projection;

	/// <summary>Creates a UI renderer using the specified graphics context and shaders.</summary>
	public UiRenderer(IGraphics graphics, Shader[] shaders)
	{
		_graphics = graphics;
		_batch = new QuadBatch(graphics, shaders);
		TextureManager = new UiTextureManager(graphics, this);

		Events.Subscribe<Events.SurfaceResizeEvent>(OnResize);
		OnResize(WindowSize);
	}

	/// <summary>The underlying quad batch used for rendering.</summary>
	public QuadBatch Batch => _batch;

	/// <summary>Begins a render batch with the specified texture filtering mode.</summary>
	public void Begin(TextureFiltering textureFiltering)
	{
		_filtering = textureFiltering;
		_batch.Begin(_projection, textureFiltering);
		_beginCalled = true;
	}

	/// <summary>Ends the current render batch.</summary>
	public void End()
	{
		_batch.End();
		_beginCalled = false;
	}

	/// <summary>Draws a sprite for FontStashSharp.</summary>
	public void DrawSprite(object texture, Vector2 pos, Rectangle? src,
		FSColor color, float rotation, Vector2 scale, float depth)
	{
		_batch.DrawSprite((TextureHandle)texture, pos, src, color.ToRgbaFloat(), rotation, scale);
	}

	/// <summary>Draws a quad for Myra.</summary>
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

	/// <summary>The texture manager used by UI and font atlas textures.</summary>
	public ITexture2DManager TextureManager { get; }

	/// <summary>The Myra renderer type.</summary>
	public RendererType RendererType => RendererType.Quad;

	/// <summary>The scissor rectangle used to clip draw output.</summary>
	public Rectangle Scissor
	{
		get => _batch.Scissor;
		set
		{
			if (_batch.Scissor == value) return;
			_batch.Scissor = value;
		}
	}

	/// <summary>The underlying Veldrid graphics device.</summary>
	public GraphicsDevice GraphicsDevice => _graphics.Device;

	/// <summary>The current Veldrid command list.</summary>
	public CommandList CommandList => _graphics.CommandList;

	/// <summary>The Veldrid resource factory.</summary>
	public ResourceFactory Factory => _graphics.Factory;

	/// <summary>The current viewport size.</summary>
	public Vector2 WindowSize => new(_graphics.ViewportWidth, _graphics.ViewportHeight);

	/// <summary>Begins a render batch with linear texture filtering.</summary>
	public void Begin() => Begin(TextureFiltering.Linear);

	/// <summary>Creates a resource set for the specified texture view and filtering mode.</summary>
	public ResourceSet CreateResourceSet(TextureView textureView, TextureFiltering filtering) =>
		_batch.CreateResourceSet(textureView, filtering);

	/// <summary>Disposes GPU resources owned by this renderer.</summary>
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