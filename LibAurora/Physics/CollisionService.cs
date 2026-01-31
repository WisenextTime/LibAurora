using System;
using System.Collections.Generic;
using LibAurora.Event;
using Rectangle = Raylib_cs.Rectangle;
using LibAurora.Framework;
namespace LibAurora.Physics;

public class CollisionService : IUpdatable, IInitializable, IDisposable
{
	/// <summary>
	/// Get the CollisionService singleton
	/// </summary>
	/// <exception cref="InvalidOperationException">CollisionService is not initialized.</exception>
	public static CollisionService Instance
		=> _instance ?? throw new InvalidOperationException("Collision service is not initialized.");
	public enum SpaceQueryType
	{
		QuadTree,
		SpatialGrid,
		Custom,
	}
	public ISpaceQuery? CustomSpaceQuery;
	public SpaceQueryType EnableSpaceQuery = SpaceQueryType.QuadTree;
	public CollisionService()
	{
		if(_instance != null) throw new InvalidOperationException("Collision service already been created");
		_instance = this;
	}
	private static CollisionService? _instance;
	private SpaceQueryType _enableSpaceQuery = SpaceQueryType.QuadTree;
	private ISpaceQuery _spaceQuery = null!;
	
	private bool _isInitialized;

	private List<Rectangle> _rectangles = [];

	public void Initialize()
	{
		_enableSpaceQuery = EnableSpaceQuery;
		_spaceQuery = _enableSpaceQuery switch
		{
			SpaceQueryType.QuadTree => new QuadTree(new Rectangle(-1024,-1024,2048,2048)),
			SpaceQueryType.SpatialGrid => new SpacialGrid(new Rectangle(-1024,-1024,2048,2048),128),
			_ => CustomSpaceQuery??throw new InvalidOperationException("CustomSpaceQuery is not set"),
		};
		EventServices.Instance.RegisterEvent<CollisionShapeDirtEvent>();
		EventServices.Instance.Subscribe<CollisionShapeDirtEvent>(@event =>
		{
			_spaceQuery.Dirt(@event.Shape);
		});
		_isInitialized = true;
	}
	public void Update(double delta)
	{
		if (!_isInitialized) return;
		_spaceQuery.Update(delta);
	}

	public void AddShape(CollisionShape shape)
	{
		if (!_isInitialized) return;
		_spaceQuery.AddShape(shape);
	}

	public void RemoveShape(CollisionShape shape)
	{
		if (!_isInitialized) return;
		_spaceQuery.RemoveShape(shape);
	}
	
	public void Clear()
	{
		if (!_isInitialized) return;
		_spaceQuery.Clear();
	}

	public IEnumerable<CollisionShape> GetShapes(CollisionShape shape)
	{
		return !_isInitialized ? [] : _spaceQuery.GetCollisionShapes(shape);
	}
	public void DebugDraw()
	{
		_spaceQuery.DebugDraw();
	}
	public void Dispose()
	{
		_instance = null;
	}
}