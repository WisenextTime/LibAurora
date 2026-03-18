using System.Collections.Generic;
using System.Numerics;
using LibAurora.Core;
using LibAurora.Input;
using Veldrid;
using Veldrid.Sdl2;
namespace LibAurora.Backends.Desktop;

public class DesktopInput : IInput
{
	private readonly Dictionary<MouseButton, MouseEvent> _buttonStates = new();
	private readonly Dictionary<Key, KeyEvent> _keyStates = new();
	private Vector2 _mousePosition = Vector2.Zero;
	private Sdl2Window _window;
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
	public bool IsKeyDown(Key key)
		=> _keyStates.TryGetValue(key, out var keyEvent) && keyEvent is { Down: true };
	public bool IsKeyPressed(Key key)
		=> _keyStates.TryGetValue(key, out var keyEvent) && keyEvent is { Down: true, Repeat: false };
	public bool IsMouseButtonDown(MouseButton button)
		=> _buttonStates.TryGetValue(button, out var mouseEvent) && mouseEvent is { Down: true };
	public Vector2 GetMousePosition() => _mousePosition;

	private void OnKeyDown(KeyEvent keyEvent) => _keyStates[keyEvent.Key] = keyEvent;
	private void OnKeyUp(KeyEvent keyEvent) => _keyStates[keyEvent.Key] = keyEvent;
	private void OnMouseUp(MouseEvent mouseEvent) => _buttonStates.Remove(mouseEvent.MouseButton);
	private void OnMouseMove(MouseMoveEventArgs mouseMoveEvent) => _mousePosition = mouseMoveEvent.MousePosition;
	private void OnMouseDown(MouseEvent mouseEvent) => _buttonStates[mouseEvent.MouseButton] = mouseEvent;
	private void OnMouseWheel(MouseWheelEventArgs wheelEvent)
		=> Events.Raise(new Events.MouseWheelEvent(wheelEvent.WheelDelta < 0));
}