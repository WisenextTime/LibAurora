using System;
using System.Collections.Generic;
using System.Numerics;
using LibAurora.Scene;
namespace LibAurora.Core;

/// <summary>
/// Type-safe global event bus for decoupled communication between components.
/// Supports subscribe, unsubscribe, and publish of any event type.
/// </summary>
public static class Events
{
	private readonly static Dictionary<Type, Delegate?> _eventHandlers = new();

	/// <summary>
	/// Subscribes a handler to events of type <typeparamref name="T"/>.
	/// </summary>
	public static void Subscribe<T>(Action<T> handler)
	{
		var type = typeof(T);
		_eventHandlers[type] = Delegate.Combine(_eventHandlers.GetValueOrDefault(type), handler);
	}

	/// <summary>
	/// Unsubscribes a previously registered handler from events of type <typeparamref name="T"/>.
	/// </summary>
	public static void Unsubscribe<T>(Action<T> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		var type = typeof(T);
		if (!_eventHandlers.TryGetValue(type, out var invoker) || invoker == null) return;
		_eventHandlers[type] = Delegate.Remove(invoker, handler);
	}

	/// <summary>
	/// Raises an event of type <typeparamref name="T"/>, invoking all subscribed handlers.
	/// </summary>
	public static void Raise<T>(T source)
	{
		ArgumentNullException.ThrowIfNull(source);
		if (!_eventHandlers.TryGetValue(typeof(T), out var invoker) || invoker == null) return;
		((Action<T>)invoker).Invoke(source);
	}

	/// <summary>Raised when the window/surface is resized.</summary>
	public readonly struct SurfaceResizeEvent(Vector2 size)
	{
		public readonly Vector2 Size = size;
	}

	/// <summary>Raised when the mouse wheel is scrolled. <c>IsDown</c> is true for scroll-down.</summary>
	public readonly struct MouseWheelEvent(bool isDown)
	{
		public bool IsDown => isDown;
		public bool IsUp => !isDown;
	}

	/// <summary>Raised when a character is typed via keyboard.</summary>
	public readonly struct TextInputEvent(char character)
	{
		public readonly char Character = character;
	}
}