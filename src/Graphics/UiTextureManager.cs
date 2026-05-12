using System.Collections.Generic;
using FontStashSharp.Interfaces;
using Myra.Graphics2D;
using Veldrid;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace LibAurora.Graphics;

/// <summary>Creates and updates textures used by UI and FontStashSharp.</summary>
public class UiTextureManager(IGraphics graphics, UiRenderer renderer) : ITexture2DManager
{
	/// <summary>Creates a texture with resource sets for all supported filtering modes.</summary>
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
		return new TextureHandle(texture, view, resourceSets);
	}

	/// <summary>Gets the size of the specified texture.</summary>
	public Point GetTextureSize(object texture)
	{
		var textureHandle = (TextureHandle)texture;
		return textureHandle.Size;
	}

	/// <summary>Uploads pixel data into the specified texture region.</summary>
	public unsafe void SetTextureData(object texture, Rectangle bounds, byte[] data)
	{
		var h = (TextureHandle)texture;
		fixed (byte* ptr = data)
		{
			graphics.Device.UpdateTexture(h.Texture, (nint)ptr, (uint)data.Length,
				(uint)bounds.X, (uint)bounds.Y, 0,
				(uint)bounds.Width, (uint)bounds.Height, 1, 0, 0);
		}
	}
}