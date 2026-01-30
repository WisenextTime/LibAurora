using System;
using System.Collections.Generic;
using Raylib_cs;
namespace LibAurora.Input;

public static class KeyboardInputs
{
	private readonly static KeyboardKey[] _keys = Enum.GetValues<KeyboardKey>();
	private static Dictionary<KeyboardKey,bool> _currentKeyStates = new();
	private static Dictionary<KeyboardKey,bool> _previousKeyStates = new();

	static KeyboardInputs()
	{
		foreach (var key in _keys)
		{
			_currentKeyStates[key] = false;
			_previousKeyStates[key] = false;
		}
	}
	public static void Update()
	{
		(_previousKeyStates, _currentKeyStates) = (_currentKeyStates, _previousKeyStates);
		foreach (var key in _keys)
			_currentKeyStates[key] = Raylib.IsKeyDown(key);
		
	}

	public static bool IsKeyPressed(KeyboardKey key) => _currentKeyStates[key];
	public static bool IsKeyReleased(KeyboardKey key) => !_currentKeyStates[key];
	public static bool IsKeyJustPressed(KeyboardKey key) => _currentKeyStates[key] && !_previousKeyStates[key];
	public static bool IsKeyJustReleased(KeyboardKey key) => !_currentKeyStates[key] && _previousKeyStates[key];
}

public static class MouseButtonInputs
{
	private readonly static MouseButton[] _buttons = Enum.GetValues<MouseButton>();
	private static Dictionary<MouseButton,bool> _currentMouseStates = new();
	private static Dictionary<MouseButton,bool> _previousMouseStates = new();

	static MouseButtonInputs()
	{
		foreach (var key in _buttons)
		{
			_currentMouseStates[key] = false;
			_previousMouseStates[key] = false;
		}
	}
	public static void Update()
	{
		(_previousMouseStates, _currentMouseStates) = (_currentMouseStates, _previousMouseStates);
		foreach (var button in _buttons)
			_currentMouseStates[button] = Raylib.IsMouseButtonDown(button);
		
	}
	
	public static bool IsButtonPressed(MouseButton button) => _currentMouseStates[button];
	public static bool IsButtonReleased(MouseButton button) => !_currentMouseStates[button];
	public static bool IsButtonJustPressed(MouseButton button) => _currentMouseStates[button] && !_previousMouseStates[button];
	public static bool IsButtonJustReleased(MouseButton button) => !_currentMouseStates[button] && _previousMouseStates[button];
}

public static class GamepadInputs
{
	private static List<int> _pads = [];
	private static Dictionary<int, GamepadState> _states = [];
	public class GamepadConfig(int id) : IEquatable<GamepadConfig>
	{
		public int Id { get; } = id;
		public float DeadZone { get; init; } = 0.2f;
		public bool Equals(GamepadConfig? other) => other != null && Id == other.Id;
		public override bool Equals(object? obj) => obj is GamepadConfig other && Equals(other);
		public override int GetHashCode() => HashCode.Combine(Id);
	}
	private class GamepadState
	{
		private readonly GamepadConfig _pad;
		private static GamepadButton[] _buttons = Enum.GetValues<GamepadButton>();
		private static GamepadAxis[] _axes = Enum.GetValues<GamepadAxis>();
		private Dictionary<GamepadButton,bool> _currentButtonStates = new();
		private Dictionary<GamepadButton,bool> _previousButtonStates = new();
		private Dictionary<GamepadAxis,float> _currentAxisStates = new();
		private Dictionary<GamepadAxis,float> _previousAxisStates = new();
		public GamepadState(GamepadConfig pad)
		{
			_pad = pad;
			foreach (var button in _buttons)
			{
				_currentButtonStates.Add(button, false);
				_previousButtonStates.Add(button, false);
			}
			foreach (var axis in _axes)
			{
				_currentAxisStates.Add(axis, 0f);
				_previousAxisStates.Add(axis, 0f);
			}
		}
		public void UpdateState()
		{
			(_currentButtonStates, _previousButtonStates) = (_previousButtonStates, _currentButtonStates);
			foreach (var button in _buttons)
				_currentButtonStates[button] = Raylib.IsGamepadButtonDown(_pad.Id, button);
			(_currentAxisStates, _previousAxisStates) = (_previousAxisStates, _currentAxisStates);
			foreach (var axis in _axes)
				_currentAxisStates[axis] = Raylib.GetGamepadAxisMovement(_pad.Id, axis);
		}
		public float CurrentAxisState(GamepadAxis axis){
			var value = _currentAxisStates[axis];
			var absValue = value < 0f ? -value : value;
			return absValue > _pad.DeadZone ? value : 0f;
		}
		public float PreviousAxisState(GamepadAxis axis)
		{
			var value = _previousAxisStates[axis];
			var absValue = value < 0f ? -value : value;
			return absValue > _pad.DeadZone ? value : 0f;
		}
		
		public bool IsButtonPressed(GamepadButton button) => _currentButtonStates[button];
		public bool IsButtonReleased(GamepadButton button) => !_currentButtonStates[button];
		public bool IsButtonJustPressed(GamepadButton button) => _currentButtonStates[button] && !_previousButtonStates[button];
		public bool IsButtonJustReleased(GamepadButton button) => !_currentButtonStates[button] && _previousButtonStates[button];
	}
	public static bool Register(GamepadConfig pad)
	{
		if (_pads.Contains(pad.Id) || _pads.Count >= 4) return false;
		_pads.Add(pad.Id);
		_states.Add(pad.Id, new GamepadState(pad));
		return true;
	}
	public static bool Unregister(GamepadConfig pad)
	{
		if (!_pads.Remove(pad.Id)) return false;
		_states.Remove(pad.Id);
		return true;
	}

	public static void Update()
	{
		foreach (var state in _states.Values)
		{
			state.UpdateState();
		}
	}

	public static bool IsButtonJustPressed(int id, GamepadButton button)
	{
		var state = _states.GetValueOrDefault(id);
		return state is not null && state.IsButtonJustPressed(button);
	}
	public static bool IsButtonJustReleased(int id, GamepadButton button)
	{
		var state = _states.GetValueOrDefault(id);
		return state is not null && state.IsButtonJustReleased(button);
	}
	public static bool IsButtonPressed(int id, GamepadButton button)
	{
		var state = _states.GetValueOrDefault(id);
		return state is not null && state.IsButtonPressed(button);
	}
	public static bool IsButtonReleased(int id, GamepadButton button)
	{
		var state = _states.GetValueOrDefault(id);
		return state is not null && state.IsButtonReleased(button);
	}

	public static float GetCurrentAxisValue(int id, GamepadAxis axis)
	{
		var state = _states.GetValueOrDefault(id);
		if (state is null) return 0;
		return state.CurrentAxisState(axis);
	}
	public static float GetPreviousAxisValue(int id, GamepadAxis axis)
	{
		var state = _states.GetValueOrDefault(id);
		if (state is null) return 0;
		return state.PreviousAxisState(axis);
	}
}