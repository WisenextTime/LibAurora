using System.Collections.Generic;
using FontStashSharp.Interfaces;
using Myra.Graphics2D;
using Veldrid;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
namespace LibAurora.Graphics.Myra;

/// <summary>
/// Implements <see cref="ITexture2DManager"/> for FontStashSharp font atlas textures.
/// Creates <see cref="MyraTexture"/> instances with pre-built <see cref="ResourceSet"/> entries
/// for all three <see cref="TextureFiltering"/> modes via <see cref="MyraRenderer.CreateResourceSet"/>.
/// </summary>
public class MyraTextureManager(IGraphics graphics, MyraRenderer renderer) : ITexture2DManager
{

	/// <summary>Creates a new GPU texture with pre-built resource sets for all three filtering modes.</summary>
	public object CreateTexture(int width, int height)
	{
		var texture = graphics.Factory.CreateTexture(new TextureDescription(
			(uint)width, (uint)height, 1u, 1u, 1u,
			PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
			TextureUsage.Sampled,
			TextureType.Texture2D));
		var view = graphics.Factory.CreateTextureView(new TextureViewDescription(texture));
		var resourceSets = new Dictionary<TextureFiltering, ResourceSet>
		{
			{ TextureFiltering.Nearest, renderer.CreateResourceSet(view, TextureFiltering.Nearest) },
			{ TextureFiltering.Linear, renderer.CreateResourceSet(view, TextureFiltering.Linear) },
			{ TextureFiltering.Anisotropic, renderer.CreateResourceSet(view, TextureFiltering.Anisotropic) },
		};
		return new MyraTexture(texture, view, resourceSets);
	}
	/// <summary>Returns the dimensions of the given <see cref="MyraTexture"/> in pixels.</summary>
	public Point GetTextureSize(object texture)
	{
		var myraTexture = (MyraTexture)texture;
		return myraTexture.Size;
	}
	/// <summary>Uploads pixel data to a sub-region of the given texture.</summary>
	public unsafe void SetTextureData(object texture, Rectangle bounds, byte[] data)
	{
		var h = (MyraTexture)texture;
		fixed (byte* ptr = data)
		{
			graphics.Device.UpdateTexture(h.Texture, (nint)ptr, (uint)data.Length,
				(uint)bounds.X, (uint)bounds.Y, 0,
				(uint)bounds.Width, (uint)bounds.Height, 1, 0, 0);
		}
	}
}