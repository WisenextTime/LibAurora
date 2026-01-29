using System;
using System.Collections.Generic;
using System.Linq;
using LibAurora.Utils;
using Raylib_cs;
namespace LibAurora.Physics;

public class SpacialGrid : ISpaceQuery
{
	private readonly Rectangle _boundary;
	private readonly int _cellSize;
	private readonly int _gridWidth;
	private readonly int _gridHeight;
	private List<CollisionShape> _shapes = [];
	private List<CollisionShape> _dirtShapes = [];
	private struct GridCell()
	{
		private List<CollisionShape> Shapes
		{
			get => field ??= [];
		} = [];
		public void AddShape(CollisionShape shape) => Shapes.Add(shape);
		public void RemoveShape(CollisionShape shape) => Shapes.Remove(shape);
		public IEnumerable<CollisionShape> GetCollisionShapes(CollisionShape target)
			=> Shapes.Where(shape => shape.IsCollision(target));
		public void Clear() => Shapes.Clear();

		public void DebugDraw()
		{
			foreach (var shape in Shapes)
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
		}
	}
	private readonly GridCell[,] _grid;
	public SpacialGrid(Rectangle boundary, int cellSize)
	{
		if (boundary.Width % cellSize != 0 || boundary.Height % cellSize != 0)
			throw new ArgumentException("Boundary dimensions must be evenly divisible by cell size", nameof(boundary));
		_boundary = boundary;
		_cellSize = cellSize;
		int x = (int)boundary.Width / cellSize, y = (int)boundary.Height / cellSize;
		_gridWidth = x;
		_gridHeight = y;
		_grid = new GridCell[x, y];
	}
	public void AddShape(CollisionShape shape)
	{
		var cells = GetCellsFromBounds(shape.GetBounds());
		foreach (var (x, y) in cells)
		{
			_grid[x, y].AddShape(shape);
		}
		_shapes.Add(shape);
	}
	public void RemoveShape(CollisionShape shape)
	{
		var cells = GetCellsFromBounds(shape.GetBounds());
		foreach (var (x, y) in cells)
		{
			_grid[x, y].RemoveShape(shape);
		}
		_shapes.Remove(shape);
	}
	public void Clear()
	{
		foreach (var gridCell in _grid)
		{
			gridCell.Clear();
		}
	}
	public IEnumerable<CollisionShape> GetCollisionShapes(CollisionShape shape)
	{
		var result = new List<CollisionShape>();
		
		var cells = GetCellsFromBounds(shape.GetBounds());
		foreach (var (x, y) in cells)
		{
			result.AddRange(_grid[x, y].GetCollisionShapes(shape));
		}
		return result.Distinct();
	}
	public void Update(double delta)
	{
		foreach (var shape in _dirtShapes)
		{
			RemoveShape(shape);
			AddShape(shape);
		}
		_dirtShapes.Clear();
	}
	public void DebugDraw()
	{
		var width = (int)_boundary.Width/_cellSize;
		var height = (int)_boundary.Height/_cellSize;
		
		for (var x = 0; x < width; x++)
		{
			for (var y = 0; y < height; y++)
			{
				Raylib.DrawRectangleLinesEx(
					new Rectangle(_boundary.X + x * _cellSize, _boundary.Y + y * _cellSize, _cellSize, _cellSize),
					2,
					Color.White);
				_grid[x,y].DebugDraw();
			}
		}
	}
	public void Dirt(CollisionShape? shape)
	{
		if(shape is null) return;
		if (!_shapes.Contains(shape) || _dirtShapes.Contains(shape)) return;
		_dirtShapes.Add(shape);
	}
	private (int x, int y) WorldToGrid(float worldX, float worldY)
	{
		var gridX = (int)((worldX - _boundary.X) / _cellSize);
		var gridY = (int)((worldY - _boundary.Y) / _cellSize);
		
		gridX = Math.Clamp(gridX, 0, _gridWidth - 1);
		gridY = Math.Clamp(gridY, 0, _gridHeight - 1);
		
		return (gridX, gridY);
	}
	private List<(int, int)> GetCellsFromBounds(Rectangle bounds)
	{
		var cells = new List<(int, int)>();
		
		var (minX, minY) = WorldToGrid(bounds.X, bounds.Y);
		var (maxX, maxY) = WorldToGrid(bounds.X + bounds.Width, bounds.Y + bounds.Height);
		
		minX = Math.Clamp(minX, 0, _gridWidth - 1);
		maxX = Math.Clamp(maxX, 0, _gridWidth - 1);
		minY = Math.Clamp(minY, 0, _gridHeight - 1);
		maxY = Math.Clamp(maxY, 0, _gridHeight - 1);
		
		for (var x = minX; x <= maxX; x++)
		{
			for (var y = minY; y <= maxY; y++)
			{
				cells.Add((x, y));
			}
		}
		
		return cells;
	}
}