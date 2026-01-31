using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LibAurora.Framework;
using Raylib_cs;
namespace LibAurora.Input;

public sealed class InputService : IUpdatable, IDisposable
{
	/// <summary>
	/// Get the InputService singleton
	/// </summary>
	/// <exception cref="InvalidOperationException">InputService is not initialized.</exception>
	public static InputService Instance => _instance ?? throw new InvalidOperationException("Input service is not initialized.");
	public InputService()
	{
		if(_instance != null) throw new InvalidOperationException("Input service already been created");
		_instance = this;
	}
	private static InputService? _instance;
	private sealed record ActionState
	{
		public bool Pressed;
		public bool Down;
		public bool Released;
		public bool Up;
		public float Strength;
		public readonly List<InputSource> Inputs = [];
	}
	private Dictionary<string, ActionState> _actions = new();
	public void Update(double deltaTime)
	{
		KeyboardInputs.Update();
		MouseButtonInputs.Update();
		GamepadInputs.Update();
		foreach (var action in _actions.Values)
		{
			action.Pressed = action.Inputs.Any(input => input.JustPressed());
			action.Down = action.Inputs.Any(input => input.Pressed());
			action.Released = action.Inputs.Any(input => input.JustReleased());
			action.Up = action.Inputs.Any(input => input.Released());
			action.Strength = action.Inputs.Max(input => input.Strength());
		}
	}
	/// <summary>
	/// Add a new action.
	/// </summary>
	/// <param name="action">Name of this action</param>
	public void AddAction(string action) => _actions.TryAdd(action, new ActionState());
	/// <summary>
	/// Bind an input to an action.
	/// </summary>
	/// <param name="source">Input source</param>
	/// <param name="action">Action name</param>
	public void BindAction(InputSource source, string action)
	{
		if(!_actions.TryGetValue(action, out var state))return; 
		if (!state.Inputs.Any(input => input.Equals(source))) state.Inputs.Add(source);
	}
	/// <summary>
	/// Remove an action
	/// </summary>
	/// <param name="action">Name of this action</param>
	public void RemoveAction(string action) => _actions.Remove(action);
	/// <summary>
	/// Debind an input from action
	/// </summary>
	/// <param name="source">input source</param>
	/// <param name="action">Action Name</param>
	public void DebindAction(InputSource source, string action)
	{
		if(!_actions.TryGetValue(action, out var state))return;
		state.Inputs.Remove(source);
	}
	/// <summary>
	/// Bind a keyboard input to an action
	/// </summary>
	/// <param name="key">Input key</param>
	/// <param name="action">Action name</param>
	public void BindKeyAction(KeyboardKey key, string action) => BindAction(new KeySource(key), action);
	/// <summary>
	/// Bind a mouse button input to an action
	/// </summary>
	/// <param name="button">Input mouse button</param>
	/// <param name="action">Action name</param>
	public void BindMouseAction(MouseButton button, string action) => BindAction(new MouseSource(button), action);
	/// <summary>
	/// Bind a Gamepad button input to an action
	/// </summary>
	/// <param name="button">Gamepad button input</param>
	/// <param name="action">Action name</param>
	/// <param name="padId">Gamepad Id (default 0)</param>
	public void BindPadButtonAction(GamepadButton button, string action, int padId = 0)
		=> BindAction(new PadButtonSource(padId, button), action);
	/// <summary>
	/// Bind a Gamepad axis input to an action
	/// </summary>
	/// <param name="axis">Gamepad axis input</param>
	/// <param name="action">Action name</param>
	/// <param name="padId">Gamepad Id (default zero)</param>
	public void BindPadAxisAction(GamepadAxis axis, string action, int padId = 0)
		=> BindAction(new PadAxisSource(padId, axis), action);
	/// <summary>
	/// Check whether an action is currently triggerred.
	/// </summary>
	/// <param name="action">Action name</param>
	/// <returns></returns>
	public bool IsActionDown(string action) => _actions.ContainsKey(action) && _actions[action].Down;
	/// <summary>
	/// Check whether an action is not currently triggerred.
	/// </summary>
	/// <param name="action">Action name</param>
	/// <returns></returns>
	public bool IsActionUp(string action) => _actions.ContainsKey(action) && _actions[action].Up;
	/// <summary>
	/// Check whether an action has just been triggerred.
	/// </summary>
	/// <param name="action">Action name</param>
	/// <returns></returns>
	public bool IsActionPressed(string action) => _actions.ContainsKey(action) && _actions[action].Pressed;
	/// <summary>
	/// Check whether an action has just been not triggerred.
	/// </summary>
	/// <param name="action">Action name</param>
	/// <returns></returns>
	public bool IsActionReleased(string action) => _actions.ContainsKey(action) && _actions[action].Released;
	/// <summary>
	/// Check the action strength (if available).
	/// </summary>
	/// <param name="action">Action name</param>
	/// <returns></returns>
	public float GetActionStrength(string action) => _actions.ContainsKey(action) ? _actions[action].Strength : 0;
	/// <summary>
	/// Get a vector2 with four directions
	/// </summary>
	/// <param name="up">up direction action</param>
	/// <param name="down">down direction action</param>
	/// <param name="left">left direction action</param>
	/// <param name="right">right direction action</param>
	/// <returns></returns>
	public Vector2 GetAxisJoy(string up, string down, string left, string right)
	{
		var value = new Vector2
		{
			X = (IsActionDown(left) ? -1 : 0) + (IsActionDown(right) ? 1 : 0),
			Y = (IsActionDown(up) ? -1 : 0) + (IsActionDown(down) ? 1 : 0),
		};
		return value;
	}
	/// <summary>
	/// Get a vector2 with two axes.
	/// </summary>
	/// <param name="upDown">up-down direction axis</param>
	/// <param name="leftRight">left-right direction axis</param>
	/// <returns></returns>
	public Vector2 GetAxisJoy(string upDown, string leftRight)
	{
		var value = new Vector2
		{
			X = GetActionStrength(upDown),
			Y = GetActionStrength(leftRight),
		};
		return value;
	}
	
	public bool RegisterGamePadId(GamepadInputs.GamepadConfig pad) => GamepadInputs.Register(pad);
	public bool UnregisterGamePadId(GamepadInputs.GamepadConfig pad) => GamepadInputs.Unregister(pad);
	public void Dispose()
	{
		_instance = null;
	}
}