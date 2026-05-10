using FontStashSharp;
using Veldrid;
namespace LibAurora.MathUtils;

public static class ColorExtension
{
	extension(FSColor fsColor)
	{
		public RgbaFloat ToRgbaFloat()
		{
			fsColor.Deconstruct(out var r, out var g, out var b, out float a);
			return new RgbaFloat(r, g, b, a);
		}
	}
}