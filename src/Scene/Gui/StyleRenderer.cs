using System.Drawing;
using System.Numerics;
using LibAurora.Graphics;
using Veldrid;
namespace LibAurora.Scene.Gui;

/// <summary>
/// Abstract renderer for drawing GUI styles, textures, and text.
/// Manages clip regions and delegates actual drawing to a backend-specific implementation.
/// </summary>
public abstract class StyleRenderer(IGraphics graphics) : IRenderer
{
	/// <summary>Backend graphics device.</summary>
	public GraphicsDevice GraphicsDevice => graphics.Device;
	/// <summary>Command list for recording draw commands.</summary>
	public CommandList CommandList => graphics.CommandList;
	/// <summary>Resource factory for creating GPU resources.</summary>
	public ResourceFactory Factory => graphics.Factory;
	/// <summary>Current window size in pixels.</summary>
	public Vector2 WindowSize => new(graphics.ViewportWidth, graphics.ViewportHeight);
	/// <summary>Begins a render pass.</summary>
	public abstract void Begin();
	/// <summary>Ends a render pass.</summary>
	public abstract void End();
	/// <summary>Pushes a clip rectangle. Subsequent draws are clipped to this region.</summary>
	public abstract void PushClip(uint x, uint y, uint w, uint h);
	/// <summary>Pops the current clip rectangle.</summary>
	public abstract void PopClip();
	/// <summary>Draws a <see cref="Style"/> at the specified position and size.</summary>
	public abstract void Draw(Style style, float x, float y, float w, float h, RgbaFloat? color = null);
	/// <summary>Draws a texture at the specified position and size.</summary>
	public abstract void Draw(TextureView texture, float x, float y, float w, float h, RgbaFloat? color = null);
	/// <summary>Draws text at the specified position and size.</summary>
	public abstract void DrawText(string text, float x, float y, float w, float h, uint size, RgbaFloat? color = null,
		bool autoWarp = false);

	/// <inheritdoc cref="Draw(Style, float, float, float, float, RgbaFloat?)"/>
	public void Draw(Style style, RectangleF bounds, RgbaFloat? color = null) =>
		Draw(style, bounds.X, bounds.Y, bounds.Width, bounds.Height, color);
	/// <inheritdoc cref="Draw(TextureView, float, float, float, float, RgbaFloat?)"/>
	public void Draw(TextureView texture, RectangleF bounds, RgbaFloat? color = null) =>
		Draw(texture, bounds.X, bounds.Y, bounds.Width, bounds.Height, color);
	/// <inheritdoc cref="DrawText(string, float, float, float, float, uint, RgbaFloat?, bool)"/>
	public void DrawText(string text, RectangleF bounds, uint size, RgbaFloat? color = null, bool autoWarp = false) =>
		DrawText(text, bounds.X, bounds.Y, bounds.Width, bounds.Height, size, color, autoWarp);
}
/// <summary>Base class for renderable GUI styles.</summary>
public class Style;
/// <summary>A style that renders a single texture.</summary>
public class TextureStyle(TextureView texture) : Style
{
	/// <summary>The texture to render.</summary>
	public TextureView Texture => texture;
}
/// <summary>
/// A style that renders a texture using nine-slice scaling.
/// The texture is divided into nine regions with configurable padding to preserve
/// border/corner fidelity when the element is resized.
/// </summary>
public class NineSliceTextureStyle(TextureView texture) : TextureStyle(texture)
{
	/// <summary>Padding from the bottom edge of the texture.</summary>
	public uint BottomPadding;
	/// <summary>Padding from the left edge of the texture.</summary>
	public uint LeftPadding;
	/// <summary>Padding from the right edge of the texture.</summary>
	public uint RightPadding;
	/// <summary>Padding from the top edge of the texture.</summary>
	public uint TopPadding;
}