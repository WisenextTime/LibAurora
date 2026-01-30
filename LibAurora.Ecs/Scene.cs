using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using LibAurora.Framework;
using LibAurora.Graphics;
namespace LibAurora.Ecs;

public class Scene(World world) : IUpdatable, IRenderable
{
	public World? SceneWorld { get; } = world;
	public bool Pause = false;
	public bool Visible = true;
	public int Priority = 0;
	
	private readonly List<object> _systems = [];
	private readonly List<IUpdatableSystem> _updatables = [];
	private readonly List<IRenderableSystem> _renderables = [];
	
	private FrozenSet<IUpdatableSystem>? _frozenUpdatable;
	private FrozenSet<IRenderableSystem>? _frozenRenderable;

	public Scene RegisterSystem(object system)
	{
        ArgumentNullException.ThrowIfNull(system);
        _systems.Add(system);
		if (system is IUpdatableSystem updatable) _updatables.Add(updatable);
		if (system is IRenderableSystem renderable) _renderables.Add(renderable);
		_frozenUpdatable = null;
		_frozenRenderable = null;
		return this;
	}
	public bool UnregisterSystem(object system)
	{
		ArgumentNullException.ThrowIfNull(system);
		var remove = _systems.Remove(system);
		if (!remove) return false;
		if (system is IUpdatableSystem updatable) _updatables.Remove(updatable);
		if (system is IRenderableSystem renderable) _renderables.Remove(renderable);
		_frozenUpdatable = null;
		_frozenRenderable = null;
		if (system is IDisposable disposable) disposable.Dispose();
		return true;
	}
	public void Update(double delta)
	{
		var updatables = GetFrozenUpdatables();
		foreach (var system in updatables)
			system.Update(delta, SceneWorld);
	}
	public void Draw()
	{
		var renderables = GetFrozenRenderables();
		foreach (var system in renderables)
			system.Draw(SceneWorld);
	}
	public T? GetSystem<T>() where T : class
	{
		return _systems.OfType<T>().FirstOrDefault();
	}
	private FrozenSet<IUpdatableSystem> GetFrozenUpdatables()
	{
		_frozenUpdatable ??= _updatables.ToFrozenSet();
		return _frozenUpdatable;
	}
	private FrozenSet<IRenderableSystem> GetFrozenRenderables()
	{
		_frozenRenderable ??= _renderables.ToFrozenSet();
		return _frozenRenderable;
	}
	public Action? OnPushed;
	public Action? OnPopped;
	public Action? OnDeactivated;
	public Action? OnActivated;
}