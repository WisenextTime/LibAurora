using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace LibAurora.Event;

public class EventServices : IDisposable
{
	/// <summary>
	/// Get the EventService singleton
	/// </summary>
	/// <exception cref="InvalidOperationException">CollisionService is not initialized.</exception>
	public static EventServices Instance
		=> _instance ?? throw new InvalidOperationException("Event service is not initialized.");
	/// <summary>
	/// Register a event.
	/// </summary>
	/// <typeparam name="TEvent">type of event.</typeparam>
	public void RegisterEvent<TEvent>() => _events.TryAdd(typeof(TEvent), []);
	/// <summary>
	/// Subscribe a event with custom delegate.
	/// </summary>
	/// <param name="del">custom delegate</param>
	/// <typeparam name="TEvent">type of event</typeparam>
	public void Subscribe<TEvent>(Action<TEvent> del)
	{
		if(!_events.TryGetValue(typeof(TEvent), out var delegates))return;
		delegates.Add(del);
	}
	/// <summary>
	/// Unsubscribe a event with custom delegate.
	/// </summary>
	/// <param name="del">custom delegate</param>
	/// <typeparam name="TEvent">type of event</typeparam>
	public void Unsubscribe<TEvent>(Action<TEvent> del)
	{
		if(!_events.TryGetValue(typeof(TEvent), out var delegates))return;
		delegates.Remove(del);
	}
	/// <summary>
	/// Publish a event.
	/// </summary>
	/// <param name="event">event source</param>
	/// <typeparam name="TEvent">event type</typeparam>
	public void Publish<TEvent>(TEvent @event)
	{
		if(!_events.TryGetValue(typeof(TEvent), out var delegates))return;
		foreach (var del in delegates)
		{
			del.DynamicInvoke(@event);
		}
	}
	
	public EventServices()
	{
		if(_instance != null) throw new InvalidOperationException("Event service already been created");
		_instance = this;
	}
	private static EventServices? _instance;
	private readonly ConcurrentDictionary<Type,List<Delegate>>  _events = new();
	public void Dispose()
	{
		_instance = null;
	}
}