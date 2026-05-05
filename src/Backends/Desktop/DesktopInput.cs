using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using LibAurora.Core;
using LibAurora.Input;
using Veldrid;
using Veldrid.Sdl2;
namespace LibAurora.Backends.Desktop;

/// <summary>
/// Desktop input implementation using SDL2 window events.
/// Polls keyboard, mouse button, mouse position, and mouse wheel state.
/// Uses SDL2's <c>SDL_AddEventWatch</c> to intercept text input events
/// for proper Unicode / IME character input.
/// </summary>
public class DesktopInput : IInput
{

	private const uint SdlTextInputEventType = 0x303;
	private readonly static SdlEventFilter _textInputFilter = OnTextInputEvent;
	private static bool _watchRegistered;
	private readonly Dictionary<MouseButton, MouseEvent> _buttonStates = new();
	private readonly Dictionary<Key, KeyEvent> _keyStates = new();
	private readonly HashSet<Key> _pressedKeys = [];
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
	/// <remarks>Consumed on read; returns true only once per press.</remarks>
	public bool IsKeyPressed(Key key)
		=> _pressedKeys.Remove(key);

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

	/// <summary>Sets the text input rectangle for IME candidate window positioning.</summary>
	public void SetTextInputRect(uint x, uint y, uint w, uint h)
	{
		var rect = new SdlRect { x = (int)x, y = (int)y, w = (int)w, h = (int)h };
		SDL_SetTextInputRect(ref rect);
	}

	[DllImport("SDL2")] private static extern void SDL_StartTextInput();

	[DllImport("SDL2")] private static extern void SDL_AddEventWatch(SdlEventFilter filter, IntPtr userdata);

	[DllImport("SDL2")] private static extern void SDL_SetTextInputRect(ref SdlRect rect);

	/// <summary>Enables SDL2 text input and registers the global event watch. Call once after window creation.</summary>
	public static void EnableTextInput()
	{
		SDL_StartTextInput();
		if (_watchRegistered) return;
		SDL_AddEventWatch(_textInputFilter, IntPtr.Zero);
		_watchRegistered = true;
	}

	private static unsafe int OnTextInputEvent(IntPtr userdata, IntPtr sdlevent)
	{
		var ev = (RawTextInputEvent*)sdlevent;
		if (ev->type != SdlTextInputEventType) return 1;
		var len = 0;
		while (len < 32 && ev->text[len] != 0) len++;
		if (len == 0) return 1;
		var str = Encoding.UTF8.GetString(ev->text, len);
		foreach (var c in str)
			Events.Raise(new Events.TextInputEvent(c));
		return 1;
	}

	private void OnKeyDown(KeyEvent keyEvent)
	{
		_keyStates[keyEvent.Key] = keyEvent;
		if (!keyEvent.Repeat) _pressedKeys.Add(keyEvent.Key);
	}
	private void OnKeyUp(KeyEvent keyEvent)
	{
		_keyStates[keyEvent.Key] = keyEvent;
		_pressedKeys.Remove(keyEvent.Key);
	}
	private void OnMouseUp(MouseEvent mouseEvent) => _buttonStates.Remove(mouseEvent.MouseButton);
	private void OnMouseMove(MouseMoveEventArgs mouseMoveEvent) => _mousePosition = mouseMoveEvent.MousePosition;
	private void OnMouseDown(MouseEvent mouseEvent) => _buttonStates[mouseEvent.MouseButton] = mouseEvent;
	private void OnMouseWheel(MouseWheelEventArgs wheelEvent)
	{
		_accumulatedWheelDelta += wheelEvent.WheelDelta;
		Events.Raise(new Events.MouseWheelEvent(wheelEvent.WheelDelta < 0));
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct SdlRect
	{
		public int x;
		public int y;
		public int w;
		public int h;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int SdlEventFilter(IntPtr userdata, IntPtr sdlevent);

	[StructLayout(LayoutKind.Sequential)]
	private struct RawTextInputEvent
	{
		public uint type;
		public uint timestamp;
		public uint windowID;
		public unsafe fixed byte text[32];
	}
}