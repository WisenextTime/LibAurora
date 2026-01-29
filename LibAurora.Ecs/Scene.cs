using System.Collections.Frozen;
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
	private readonly List<IUpdatable> _updatables = [];
	private readonly List<IRenderable> _renderables = [];
	
	private FrozenSet<IUpdatable>? _frozenUpdatable;
	private FrozenSet<IRenderable>? _frozenRenderable;

	public Scene RegisterSystem(object system)
	{
        ArgumentNullException.ThrowIfNull(system);
        _systems.Add(system);
		if (system is IUpdatable updatable) _updatables.Add(updatable);
		if (system is IRenderable renderable) _renderables.Add(renderable);
		_frozenUpdatable = null;
		_frozenRenderable = null;
		return this;
	}
	public bool UnregisterSystem(object system)
	{
		ArgumentNullException.ThrowIfNull(system);
		var remove = _systems.Remove(system);
		if (!remove) return false;
		if (system is IUpdatable updatable) _updatables.Remove(updatable);
		if (system is IRenderable renderable) _renderables.Remove(renderable);
		_frozenUpdatable = null;
		_frozenRenderable = null;
		if (system is IDisposable disposable) disposable.Dispose();
		return true;
	}
	public void Update(double delta)
	{
		var updatables = GetFrozenUpdatables();
		foreach (var system in updatables)
			system.Update(delta);
	}
	public void Draw()
	{
		var renderables = GetFrozenRenderables();
		foreach (var system in renderables)
			system.Draw();
	}
	public T? GetSystem<T>() where T : class
	{
		return _systems.OfType<T>().FirstOrDefault();
	}
	private FrozenSet<IUpdatable> GetFrozenUpdatables()
	{
		_frozenUpdatable ??= _updatables.ToFrozenSet();
		return _frozenUpdatable;
	}
	private FrozenSet<IRenderable> GetFrozenRenderables()
	{
		_frozenRenderable ??= _renderables.ToFrozenSet();
		return _frozenRenderable;
	}
	public Action? OnPushed;
	public Action? OnPopped;
}