using System.Drawing;
using System.Numerics;
namespace LibAurora.MathUtils;

public static class VectorExtension
{
	extension(Vector2 vector2)
	{
		public Point ToPoint() => new((int)vector2.X, (int)vector2.Y);
	}

	extension(Vector3 vector3)
	{
		public Vector2 ToVector2() => new(vector3.X, vector3.Y);
	}
}