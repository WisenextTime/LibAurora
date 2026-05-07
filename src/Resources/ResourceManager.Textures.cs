using System;
using LibAurora.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
namespace LibAurora.Resources;

public static partial class ResourceManager
{
	internal static void InitTextureProcessor(IGraphics graphics) =>
		RegisterProcesser(new ResourceProcesser<Texture>(
			load: stream =>
			{
				using var image = Image.Load<Rgba32>(stream);
				var tex = graphics.Factory.CreateTexture(new TextureDescription(
					(uint)image.Width, (uint)image.Height, 1, 1, 1,
					PixelFormat.R8_G8_B8_A8_UNorm,
					TextureUsage.Sampled,
					TextureType.Texture2D));
				var pixels = new byte[image.Width * image.Height * 4];
				image.CopyPixelDataTo(pixels);
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