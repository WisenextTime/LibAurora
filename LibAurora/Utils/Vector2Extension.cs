using System.Drawing;
using System.Numerics;
namespace LibAurora.Utils;

public static class Vector2Extension
{
	extension(Vector2 vector)
	{
		public Point ToPoint() => new((int)vector.X, (int)vector.Y);
	}
}