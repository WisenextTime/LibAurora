using System;
using System.Collections.Generic;
using System.Linq;
using LibAurora.Utils;
using Raylib_cs;
namespace LibAurora.Physics;

public class QuadTree(Rectangle bound) : ISpaceQuery
{
	public void AddShape(CollisionShape shape)
	{
		if (!_rootNode.TryAddShape(shape)) throw new ArgumentOutOfRangeException(nameof(shape));
		_shapes.Add(shape);
	}

	public void RemoveShape(CollisionShape shape)
	{
		_rootNode.RemoveShape(shape);
		_shapes.Remove(shape);
	}
	public void Clear() => _rootNode.Clear();
	public IEnumerable<CollisionShape> GetCollisionShapes(CollisionShape shape)
	{
		List<CollisionShape> shapes = [];
		_rootNode.Queue(shape, ref shapes);
		return shapes;
	}
	public void Update(double delta)
	{
		foreach (var dirt in _dirtShapes)
		{
			RemoveShape(dirt);
			AddShape(dirt);
		}
		_dirtShapes.Clear();
	}

	private sealed class QuadTreeNode(Rectangle boundary)
	{
		private const int MaxLevel = 8;
		private const int Capacity = 4;
		private readonly List<CollisionShape> _shapes = [];
		private bool _isDivided;
		private readonly QuadTreeNode?[] _subNode = new QuadTreeNode?[4];
		private int _level;

		public bool TryAddShape(CollisionShape shape)
		{
			if (!boundary.CompletelyContains(shape.GetBounds())) return false;
			if (!_isDivided && _shapes.Count >= Capacity && _level < MaxLevel) SubDivide();
			if (_isDivided)
			{
				var success = _subNode.Aggregate(false, (current, node) => current | node!.TryAddShape(shape));
				if (success) return true;
			}
			_shapes.Add(shape);
			return true;
		}
		public void Queue(CollisionShape shape,ref List<CollisionShape> found)
		{
			if (!shape.GetBounds().Contains(boundary)) return;
			found.AddRange(_shapes.Where(shape.IsCollision));
			foreach (var node in _subNode)
			{
				node?.Queue(shape, ref found);
			}
		}
		public void Clear()
		{
			_shapes.Clear();
			if (!_isDivided) return;
			for (var i = 0; i < 4; i++)
			{
				_subNode[i]?.Clear();
				_subNode[i] = null;
			}
			_isDivided = false;
		}
		public void RemoveShape(CollisionShape shape)
		{
			if (!boundary.CompletelyContains(shape.GetBounds())) return;
			if (_shapes.Remove(shape)) return;
			foreach (var node in _subNode)
			{
				node?.RemoveShape(shape);
			}
		}
		private void SubDivide()
		{
			var x = boundary.X;
			var y = boundary.Y;
			var w = boundary.Width / 2;
			var h = boundary.Height / 2;

			_subNode[0] = new QuadTreeNode(new Rectangle(x, y, w, h)) { _level = _level + 1 };
			_subNode[1] = new QuadTreeNode(new Rectangle(x + w, y, w, h)) { _level = _level + 1 };
			_subNode[2] = new QuadTreeNode(new Rectangle(x, y + h, w, h)) { _level = _level + 1 };
			_subNode[3] = new QuadTreeNode(new Rectangle(x + w, y + h, w, h)) { _level = _level + 1 };
			_isDivided = true;
			var removedShapes = (from shape in _shapes
				let success = _subNode.Aggregate(false, (current, node) => current | node!.TryAddShape(shape))
				where success
				select shape).ToList();
			_shapes.RemoveAll(removedShapes.Contains);
		}public void DebugDraw()
		{
			Raylib.DrawRectangleLinesEx(boundary, 2, Color.White);
			foreach (var shape in _shapes)
			{
				switch (shape)
				{
					case PointShape point:
						Raylib.DrawCircleV(point.Position.ToVector2(), 2, Color.SkyBlue);
						break;
					case CircleShape circle:
						Raylib.DrawCircleLinesV(circle.Position.ToVector2(),circle.Radius, Color.SkyBlue);
						break;
					case RectangleShape rect:
						Raylib.DrawRectangleLinesEx(rect.Rectangle,1, Color.SkyBlue);
						break;
				}
			}
			foreach (var node in _subNode) node?.DebugDraw();
		}
	}

	private QuadTreeNode _rootNode = new(bound);
	private List<CollisionShape> _shapes = [];
	private List<CollisionShape> _dirtShapes = [];
	
	public void DebugDraw()
	{
		_rootNode.DebugDraw();
	}
	public void Dirt(CollisionShape? shape)
	{
		if(shape is null) return;
		if (!_shapes.Contains(shape) || _dirtShapes.Contains(shape)) return;
		_dirtShapes.Add(shape);
	}
}