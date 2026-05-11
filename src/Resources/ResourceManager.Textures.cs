using System;
using LibAurora.Graphics;
using StbImageSharp;
using Veldrid;
namespace LibAurora.Resources;

public static partial class ResourceManager
{
	internal static void InitTextureProcessor(IGraphics graphics) =>
		RegisterProcesser(new ResourceProcesser<Texture>(
			load: stream =>
			{
				var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
				var tex = graphics.Factory.CreateTexture(new TextureDescription(
					(uint)image.Width, (uint)image.Height, 1, 1, 1,
					PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
					TextureUsage.Sampled,
					TextureType.Texture2D));
				var pixels = new byte[image.Width * image.Height * 4];
				image.Data.CopyTo(pixels);
				unsafe
				{
					fixed (byte* ptr = pixels)
						graphics.Device.UpdateTexture(tex, (IntPtr)ptr, (uint)pixels.Length,
							0, 0, 0, (uint)image.Width, (uint)image.Height, 1, 0, 0);
				}
				return tex;
			},
			save: (_, _) => throw new NotSupportedException()
		));
}