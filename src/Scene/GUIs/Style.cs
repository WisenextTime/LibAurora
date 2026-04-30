using Veldrid;
namespace LibAurora.Scene.GUIs;

/// <summary>
/// Base class for reusable draw styles. Provides an optional color tint
/// applied to the rendered output.
/// </summary>
public class Style
{
	/// <summary>Optional RGBA tint color. When set, each pixel is multiplied by this color.</summary>
	public RgbaFloat? Tint { get; set; }
}
/// <summary>
/// Style for drawing a 9-slice (9-patch) texture. Edge and corner regions
/// are preserved at their original size while the center is stretched.
/// </summary>
public class Style9Slice : Style
{
	/// <summary>The texture to draw.</summary>
	public TextureView Texture { get; set; } = null!;

	/// <summary>Width of the left edge slice in pixels.</summary>
	public uint Left { get; set; }

	/// <summary>Width of the right edge slice in pixels.</summary>
	public uint Right { get; set; }

	/// <summary>Height of the top edge slice in pixels.</summary>
	public uint Top { get; set; }

	/// <summary>Height of the bottom edge slice in pixels.</summary>
	public uint Bottom { get; set; }
}
/// <summary>
/// Style for drawing a texture stretched to fill a rectangle.
/// </summary>
public class StyleTexture : Style
{
	/// <summary>The texture to draw.</summary>
	public TextureView Texture { get; set; } = null!;
}