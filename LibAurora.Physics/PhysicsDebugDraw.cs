using System.Collections.Concurrent;
using System.Numerics;
using Box2D.NetStandard.Common;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using LibAurora.Graphics;
using Raylib_cs;

using Transform = Box2D.NetStandard.Common.Transform;
using Color = Box2D.NetStandard.Dynamics.World.Color;
namespace LibAurora.Physics;

public class PhysicsDebugDraw : DebugDraw, IRenderable
{
	private readonly ConcurrentQueue<DrawCommand> _drawQueue = new();
	
	private static float PPM => PhysicsService.Instance.PixelsPerMeter;
	public void Draw()
	{
		foreach (var command in _drawQueue)
		{
			var sourceColor = command.Color;
			var color = new Raylib_cs.Color(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A);
			switch (command.Type)
			{
				case DrawCommandType.Point:
					Raylib.DrawCircleV(command.Position*PPM, 2, color);
					break;
				case DrawCommandType.Segment:
					Raylib.DrawLineV(command.Vertices![0]*PPM, command.Vertices[1]*PPM, color);
					break;
				case DrawCommandType.Polygon:
				case DrawCommandType.SolidPolygon:
					for (var i = 0; i < command.Vertices!.Length; i++)
					{
						var next = i + 1;
						if (next >= command.Vertices.Length) next = 0;
						var pointA = command.Vertices[i]*PPM;
						var pointB = command.Vertices[next]*PPM;
						Raylib.DrawLineV(pointA, pointB, color);
					}
					break;
				case DrawCommandType.Circle:
				case DrawCommandType.SolidCircle:
					Raylib.DrawCircleLinesV(command.Position*PPM, command.Radius*PPM, color);
					break;
				case DrawCommandType.Transform:
					var xf = command.Transform;
					var position = xf.p * PPM;
					var angle = MathF.Atan2(xf.q.M21, xf.q.M11);
					Raylib.DrawCircleV(position, 5, Raylib_cs.Color.Red);
					var xAxis = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 20;
					Raylib.DrawLineEx(position, position + xAxis, 2, Raylib_cs.Color.Red);
					Raylib.DrawCircleV(position + xAxis, 3, Raylib_cs.Color.Red);
					var yAxis = new Vector2(
						MathF.Cos(angle + MathF.PI / 2),
						MathF.Sin(angle + MathF.PI / 2)) * 20;
					Raylib.DrawLineEx(position, position + yAxis, 2, Raylib_cs.Color.Green);
					Raylib.DrawCircleV(position + yAxis, 3, Raylib_cs.Color.Green);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		_drawQueue.Clear();
	}
	public override void DrawTransform(in Transform xf)
	{
		_drawQueue.Enqueue(new DrawCommand(xf));
	}
	public override void DrawPoint(in Vector2 position, float size, in Color color)
	{
		_drawQueue.Enqueue(new DrawCommand(position, size, color));
	}
	[Obsolete("Look out for new calls using Vector2")]
	public override void DrawPolygon(in Vec2[] vertices, int vertexCount, in Color color)
	{
		var verts = new Vector2[vertexCount];
		for (var i = 0; i < vertexCount; i++)
		{
			verts[i] = new Vector2(vertices[i].X, vertices[i].Y);
		}
		_drawQueue.Enqueue(new DrawCommand(verts, color, false));
	}
	[Obsolete("Look out for new calls using Vector2")]
	public override void DrawSolidPolygon(in Vec2[] vertices, int vertexCount, in Color color)
	{
		var verts = new Vector2[vertexCount];
		for (var i = 0; i < vertexCount; i++)
		{
			verts[i] = new Vector2(vertices[i].X, vertices[i].Y);
		}
		_drawQueue.Enqueue(new DrawCommand(verts, color, true));
	}
	[Obsolete("Look out for new calls using Vector2")]
	public override void DrawCircle(in Vec2 center, float radius, in Color color)
	{
		var vecCenter = new Vector2(center.X, center.Y);
		_drawQueue.Enqueue(new DrawCommand(vecCenter, radius, Vector2.UnitX, color, false));
	}
	[Obsolete("Look out for new calls using Vector2")]
	public override void DrawSolidCircle(in Vec2 center, float radius, in Vec2 axis, in Color color)
	{
		var vecCenter = new Vector2(center.X, center.Y);
		_drawQueue.Enqueue(new DrawCommand(vecCenter, radius, Vector2.UnitX, color, true));
	}
	[Obsolete("Look out for new calls using Vector2")]
	public override void DrawSegment(in Vec2 p1, in Vec2 p2, in Color color)
	{
		var vecP1 = new Vector2(p1.X, p1.Y);
		var vecP2 = new Vector2(p2.X, p2.Y);
		_drawQueue.Enqueue(new DrawCommand(vecP1, vecP2, color));
	}
	

	public struct DrawCommand
	{
		public readonly DrawCommandType Type;
		public readonly Vector2[]? Vertices;
		public readonly Vector2 Position;
		public readonly Vector2 Axis;
		public readonly float Size;
		public readonly float Radius;
		public readonly Color Color;
		public readonly Transform Transform;

		public DrawCommand (Vector2 position, float size, Color color)
		{
			Type = DrawCommandType.Point;
			Position = position;
			Size = size;
			Color = color;
			Vertices = null;
			Axis = Vector2.Zero;
			Radius = 0;
			Transform = default;
		}
		public DrawCommand(Vector2 p1, Vector2 p2, Color color)
		{
			Type = DrawCommandType.Segment;
			Vertices = [p1, p2];
			Color = color;
			Position = Vector2.Zero;
			Axis = Vector2.Zero;
			Size = 0;
			Radius = 0;
			Transform = default;
		}
		public DrawCommand(Vector2[] vertices, Color color, bool solid)
		{
			Type = solid ? DrawCommandType.SolidPolygon : DrawCommandType.Polygon;
			Vertices = vertices;
			Color = color;
			Position = Vector2.Zero;
			Axis = Vector2.Zero;
			Size = 0;
			Radius = 0;
			Transform = default;
		}
		public DrawCommand(Vector2 center, float radius, Vector2 axis, Color color, bool solid)
		{
			Type = solid ? DrawCommandType.SolidCircle : DrawCommandType.Circle;
			Position = center;
			Radius = radius;
			Axis = axis;
			Color = color;
			Vertices = null;
			Size = 0;
			Transform = default;
		}
		public DrawCommand(in Transform transform)
		{
			Type = DrawCommandType.Transform;
			Transform = transform;
			Position = Vector2.Zero;
			Vertices = null;
			Axis = Vector2.Zero;
			Size = 0;
			Radius = 0;
			Color = default;
		}
	}
	public enum DrawCommandType
	{
		Point,
		Segment,
		Polygon,
		SolidPolygon,
		Circle,
		SolidCircle,
		Transform
	}
}