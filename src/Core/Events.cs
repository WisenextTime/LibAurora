using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using LibAurora.Debug;
namespace LibAurora.Core;

public static class Events
{
	private readonly static ConcurrentDictionary<Type, List<Delegate>> _eventHandlers = new();
	public static void Subscribe<T>(Action<T> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		var list = _eventHandlers.GetOrAdd(typeof(T), []);
		lock (list) list.Add(handler);
	}
	public static void Unsubscribe<T>(Action<T> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		_eventHandlers.TryGetValue(typeof(T), out var list);
		if (list is null) return;
		lock (list)
		{
			list.Remove(handler);
			if (list.Count == 0) _eventHandlers.TryRemove(typeof(T), out _);
		}

	}
	public static void Raise<T>(T source)
	{
		ArgumentNullException.ThrowIfNull(source);
		_eventHandlers.TryGetValue(typeof(T), out var list);
		if (list is null) return;
		List<Delegate> cache;
		lock (list) cache = [..list];
		foreach (var handler in cache)
		{
			try
			{
				(handler as Action<T>)?.Invoke(source);
			}
			catch (Exception e)
			{
				LogServer.Log(e.ToString(), LogLevel.Error);
			}
		}
	}

	public readonly struct SurfaceResizeEvent(Vector2 size)
	{
		public readonly Vector2 Size = size;
	}

	public readonly struct SurfaceCloseEvent;

	public readonly struct MouseWheelEvent(bool direction)
	{
		public bool IsDown => direction;
		public bool IsUp => !direction;
	}
}