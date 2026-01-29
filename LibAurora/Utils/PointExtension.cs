using System.Drawing;
using System.Numerics;
namespace LibAurora.Utils;

public static class PointExtension
{
	extension(Point point)
	{
		public Vector2 ToVector2() => new(point.X, point.Y);

	}
}