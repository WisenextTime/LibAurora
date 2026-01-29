using System.Drawing;
using System.Numerics;
using Raylib_cs;
using Rectangle = Raylib_cs.Rectangle;
namespace LibAurora.Utils;

public static class RectangleExtension
{
	extension(Rectangle rectangle)
	{
		public static Rectangle NewRectangle(Point position, int width, int height)
			=> new Rectangle(position.X, position.Y, width, height);
		public float Top() => rectangle.Y;
		public float Left() => rectangle.X;
		public float Right() => rectangle.X + rectangle.Width;
		public float Bottom() => rectangle.Y + rectangle.Height;

		public bool Contains(Rectangle rect) => Raylib.CheckCollisionRecs(rectangle, rect);
		public bool Contains(Vector2 point) => Raylib.CheckCollisionPointRec(point, rectangle);
		public bool Contains(Point point) => Raylib.CheckCollisionPointRec(point.ToVector2(), rectangle);

		public bool CompletelyContains(Rectangle rect)
		{
			return rectangle.Top() <= rect.Top() &&
			       rectangle.Left() <= rect.Left() &&
			       rectangle.Right() > rect.Right() &&
			       rectangle.Bottom() > rect.Bottom();
		}
	}
}