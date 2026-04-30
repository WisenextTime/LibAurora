using Veldrid;
using LibAurora.Graphics;
namespace LibAurora.Scene.GUIs;

/// <summary>
/// Abstract renderer for GUI primitives. Extends <see cref="Renderer"/> with drawing methods
/// for rectangles, textures, 9-slice sprites, text, lines, and clip region management.
/// Concrete implementations provide the actual GPU rendering logic.
/// </summary>
public abstract class StyleRenderer(IGraphics graphics) : Renderer(graphics)
{
	/// <summary>Draws a filled rectangle.</summary>
	public abstract void DrawRect(uint x, uint y, uint w, uint h, RgbaFloat color);

	/// <summary>Draws a rectangle outline with the given thickness.</summary>
	public abstract void DrawBorder(uint x, uint y, uint w, uint h, float thickness, RgbaFloat color);

	/// <summary>Draws a texture stretched to the target rectangle, with optional tint.</summary>
	public abstract void DrawTexture(uint x, uint y, uint w, uint h, StyleTexture texture);

	/// <summary>Draws a 9-slice scaled texture, preserving edge/corner proportions.</summary>
	public abstract void Draw9Slice(uint x, uint y, uint w, uint h, Style9Slice nineSlice);

	/// <summary>Draws a string with the specified font size and color.</summary>
	public abstract void DrawText(uint x, uint y, string text, float fontSize, RgbaFloat color);

	/// <summary>Measures the pixel dimensions of a string at the given font size.</summary>
	public abstract (uint Width, uint Height) MeasureText(string text, float fontSize);

	/// <summary>Draws a line segment between two points.</summary>
	public abstract void DrawLine(uint x1, uint y1, uint x2, uint y2, float thickness, RgbaFloat color);

	/// <summary>Pushes a rectangular clip region onto the clip stack.</summary>
	public abstract void PushClip(uint x, uint y, uint w, uint h);

	/// <summary>Pops the topmost clip region from the clip stack.</summary>
	public abstract void PopClip();

	/// <summary>Resets clipping to the full viewport (restores after all pops).</summary>
	public abstract void ResetClip();
}