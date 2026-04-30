using System;
using System.Collections.Generic;
using System.Linq;
using LibAurora.Debug;
namespace LibAurora.Scene;

/// <summary>
/// Stack-based scene manager. Handles scene push/pop (deferred via queue),
/// and dispatches Update, Draw, and HandleInput to all enabled scenes in stack order.
/// </summary>
public class SceneManager
{
	private readonly Queue<Action> _pendingOperations = [];
	private readonly Stack<Scene> _scenes = [];

	/// <summary>Enqueues pushing a new scene onto the stack.</summary>
	public void Push(Scene scene) => _pendingOperations.Enqueue(() => _scenes.Push(scene));

	/// <summary>Enqueues popping the top scene from the stack.</summary>
	public void Pop() => _pendingOperations.Enqueue(() => _scenes.Pop());

	/// <summary>Executes all pending push/pop operations.</summary>
	public void PerformActions()
	{
		while (_pendingOperations.Count > 0) _pendingOperations.Dequeue().Invoke();
	}

	/// <summary>Updates all enabled scenes top-to-bottom, stopping input propagation once handled.</summary>
	public void Update(double delta)
	{
		var inputHandled = false;
		foreach (var scene in _scenes.Where(s => s.Enabled))
		{
			try
			{
				scene.Update(delta);
				if (!inputHandled) inputHandled = scene.HandleInput();
			}
			catch (Exception e)
			{
				LogServer.Log(e.ToString(), LogLevel.Error);
			}
		}
	}

	/// <summary>Draws all visible scenes bottom-to-top (reverse stack order).</summary>
	public void Draw(double alpha)
	{
		foreach (var scene in _scenes.Where(s => s.Visible).Reverse())
		{
			try
			{
				scene.Draw(alpha);
			}
			catch (Exception e)
			{
				LogServer.Log(e.ToString(), LogLevel.Error);
			}
		}
	}
}