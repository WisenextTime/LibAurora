using System;
using System.Drawing;
using LibAurora.Event;
using LibAurora.Utils;
using Raylib_cs;
using Rectangle = Raylib_cs.Rectangle;
namespace LibAurora.Physics;

public abstract class CollisionShape : IEquatable<CollisionShape>
{
	private static int _nextId;
	protected static ObjectPool<CollisionShapeDirtEvent> DirtEventPool = new(10);
	public int Id { get; } = _nextId++;
	public bool Equals(CollisionShape? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;
		return Id == other.Id;
	}
	public abstract bool IsCollision(CollisionShape? other);
	public abstract int GetApproximateRadius();
	public abstract Point GetCenter();
	public abstract Rectangle GetBounds();

	public override bool Equals(object? obj) => obj is not null && Equals(obj as CollisionShape);
	public override int GetHashCode() => Id.GetHashCode();

}

public class PointShape(Point position) : CollisionShape
{
	public Point Position
	{
		get;
		set
		{
			field = value;
			var @event = DirtEventPool.Get();
			@event.Shape = this;
			EventServices.Instance.Publish(@event);
		}
	} = position;

	public override bool IsCollision(CollisionShape? other)
	{
		if (other is null) return false;
		return other switch
		{
			PointShape point => point.Position == Position,
			CircleShape circle => Raylib.CheckCollisionPointCircle(Position.ToVector2(), circle.Position.ToVector2(), circle.Radius),
			RectangleShape rect => Raylib.CheckCollisionPointRec(Position.ToVector2(), rect.Rectangle),
			_ => throw new Exception("Unknown collision shape"),
		};
	}
	public override int GetApproximateRadius() => 0;
	public override Point GetCenter()
		=> Position;
	public override Rectangle GetBounds()
		=> Rectangle.NewRectangle(Position, 1,1);
}

public class CircleShape(Point position, int radius) : CollisionShape
{
	public Point Position
	{
		get;
		set
		{
			field = value;
			var @event = DirtEventPool.Get();
			@event.Shape = this;
			EventServices.Instance.Publish(@event);
		}
	} = position;

	public int Radius
	{
		get;
		set
		{
			field = value;
			var @event = DirtEventPool.Get();
			@event.Shape = this;
			EventServices.Instance.Publish(@event);
		}
	} = radius;

	public override bool IsCollision(CollisionShape? other)
	{
		if (other is null) return false;
		return other switch
		{
			PointShape point => Raylib.CheckCollisionPointCircle(point.Position.ToVector2(), Position.ToVector2(), Radius),
			CircleShape circle => Raylib.CheckCollisionCircles(Position.ToVector2(), Radius, circle.Position.ToVector2(), circle.Radius),
			RectangleShape rect => Raylib.CheckCollisionCircleRec(Position.ToVector2(), Radius, rect.Rectangle),
			_ => throw new Exception("Unknown collision shape"),
		};
	}
	public override int GetApproximateRadius() => Radius;
	public override Point GetCenter()
		=> Position;
	public override Rectangle GetBounds()
		=> new Rectangle(Position.X - Radius, Position.Y - Radius, Radius * 2, Radius * 2);
}

public class RectangleShape (Point position, Point size): CollisionShape
{
	public Rectangle Rectangle
	{
		get;
		set
		{
			field = value;
			var @event = DirtEventPool.Get();
			@event.Shape = this;
			EventServices.Instance.Publish(@event);
		}
	} = new Rectangle(position.ToVector2(), size.ToVector2());

	public override bool IsCollision(CollisionShape? other)
	{
		if (other is null) return false;
		return other switch
		{
			PointShape point => Raylib.CheckCollisionPointRec(point.Position.ToVector2(), Rectangle),
			CircleShape circle => Raylib.CheckCollisionCircleRec(circle.Position.ToVector2(), circle.Radius, Rectangle),
			RectangleShape rect => Raylib.CheckCollisionRecs(rect.Rectangle, Rectangle),
			_ => throw new Exception("Unknown collision shape"),
		};
	}
	public override int GetApproximateRadius() => (int)(Rectangle.Width + Rectangle.Height) / 2;
	public override Point GetCenter()
		=> Rectangle.Center.ToPoint();
	public override Rectangle GetBounds()
		=> Rectangle;
}