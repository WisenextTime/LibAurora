using System.Collections.Generic;
using System.Numerics;
using LibAurora.Core;
using LibAurora.Input;
using Veldrid;
using Veldrid.Sdl2;
namespace LibAurora.Backends.Desktop;

/// <summary>
/// Desktop input implementation using SDL2 window events.
/// Polls keyboard, mouse button, mouse position, and mouse wheel state.
/// </summary>
public class DesktopInput : IInput
{
	private readonly Dictionary<MouseButton, MouseEvent> _buttonStates = new();
	private readonly Dictionary<Key, KeyEvent> _keyStates = new();
	private float _accumulatedWheelDelta;
	private Vector2 _mousePosition = Vector2.Zero;
	private Sdl2Window _window;

	/// <summary>Creates a new input source bound to the given SDL2 window.</summary>
	public DesktopInput(Sdl2Window window)
	{
		_window = window;
		_window.KeyDown += OnKeyDown;
		_window.KeyUp += OnKeyUp;
		_window.MouseDown += OnMouseDown;
		_window.MouseUp += OnMouseUp;
		_window.MouseMove += OnMouseMove;
		_window.MouseWheel += OnMouseWheel;
	}

	/// <inheritdoc />
	public bool IsKeyDown(Key key)
		=> _keyStates.TryGetValue(key, out var keyEvent) && keyEvent is { Down: true };

	/// <inheritdoc />
	public bool IsKeyPressed(Key key)
		=> _keyStates.TryGetValue(key, out var keyEvent) && keyEvent is { Down: true, Repeat: false };

	/// <inheritdoc />
	public bool IsMouseButtonDown(MouseButton button)
		=> _buttonStates.TryGetValue(button, out var mouseEvent) && mouseEvent is { Down: true };

	/// <inheritdoc />
	public Vector2 GetMousePosition() => _mousePosition;

	/// <inheritdoc />
	public float GetMouseWheelDelta()
	{
		var delta = _accumulatedWheelDelta;
		_accumulatedWheelDelta = 0;
		return delta;
	}

	private void OnKeyDown(KeyEvent keyEvent) => _keyStates[keyEvent.Key] = keyEvent;
	private void OnKeyUp(KeyEvent keyEvent) => _keyStates[keyEvent.Key] = keyEvent;
	private void OnMouseUp(MouseEvent mouseEvent) => _buttonStates.Remove(mouseEvent.MouseButton);
	private void OnMouseMove(MouseMoveEventArgs mouseMoveEvent) => _mousePosition = mouseMoveEvent.MousePosition;
	private void OnMouseDown(MouseEvent mouseEvent) => _buttonStates[mouseEvent.MouseButton] = mouseEvent;
	private void OnMouseWheel(MouseWheelEventArgs wheelEvent)
	{
		_accumulatedWheelDelta += wheelEvent.WheelDelta;
		Events.Raise(new Events.MouseWheelEvent(wheelEvent.WheelDelta < 0));
	}
}