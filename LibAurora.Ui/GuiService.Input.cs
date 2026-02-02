using ImGuiNET;
using LibAurora.Event;
using LibAurora.Input;
using Raylib_cs;
namespace LibAurora.Ui;

public partial class GuiService
{
	private readonly static Dictionary<KeyboardKey, ImGuiKey> _keyboardMap = new()
	{
		{ KeyboardKey.Tab, ImGuiKey.Tab },
		{ KeyboardKey.Left, ImGuiKey.LeftArrow },
		{ KeyboardKey.Right, ImGuiKey.RightArrow },
		{ KeyboardKey.Up, ImGuiKey.UpArrow },
		{ KeyboardKey.Down, ImGuiKey.DownArrow },
		{ KeyboardKey.PageUp, ImGuiKey.PageUp },
		{ KeyboardKey.PageDown, ImGuiKey.PageDown },
		{ KeyboardKey.Home, ImGuiKey.Home },
		{ KeyboardKey.End, ImGuiKey.End },
		{ KeyboardKey.Insert, ImGuiKey.Insert },
		{ KeyboardKey.Delete, ImGuiKey.Delete },
		{ KeyboardKey.Backspace, ImGuiKey.Backspace },
		{ KeyboardKey.Space, ImGuiKey.Space },
		{ KeyboardKey.Enter, ImGuiKey.Enter },
		{ KeyboardKey.Escape, ImGuiKey.Escape },

		{ KeyboardKey.LeftControl, ImGuiKey.LeftCtrl },
		{ KeyboardKey.LeftShift, ImGuiKey.LeftShift },
		{ KeyboardKey.LeftAlt, ImGuiKey.LeftAlt },
		{ KeyboardKey.LeftSuper, ImGuiKey.LeftSuper },
		{ KeyboardKey.RightControl, ImGuiKey.RightCtrl },
		{ KeyboardKey.RightShift, ImGuiKey.RightShift },
		{ KeyboardKey.RightAlt, ImGuiKey.RightAlt },
		{ KeyboardKey.RightSuper, ImGuiKey.RightSuper },
		{ KeyboardKey.KeyboardMenu, ImGuiKey.Menu },

		{ KeyboardKey.Zero, ImGuiKey._0 },
		{ KeyboardKey.One, ImGuiKey._1 },
		{ KeyboardKey.Two, ImGuiKey._2 },
		{ KeyboardKey.Three, ImGuiKey._3 },
		{ KeyboardKey.Four, ImGuiKey._4 },
		{ KeyboardKey.Five, ImGuiKey._5 },
		{ KeyboardKey.Six, ImGuiKey._6 },
		{ KeyboardKey.Seven, ImGuiKey._7 },
		{ KeyboardKey.Eight, ImGuiKey._8 },
		{ KeyboardKey.Nine, ImGuiKey._9 },

		{ KeyboardKey.A, ImGuiKey.A },
		{ KeyboardKey.B, ImGuiKey.B },
		{ KeyboardKey.C, ImGuiKey.C },
		{ KeyboardKey.D, ImGuiKey.D },
		{ KeyboardKey.E, ImGuiKey.E },
		{ KeyboardKey.F, ImGuiKey.F },
		{ KeyboardKey.G, ImGuiKey.G },
		{ KeyboardKey.H, ImGuiKey.H },
		{ KeyboardKey.I, ImGuiKey.I },
		{ KeyboardKey.J, ImGuiKey.J },
		{ KeyboardKey.K, ImGuiKey.K },
		{ KeyboardKey.L, ImGuiKey.L },
		{ KeyboardKey.M, ImGuiKey.M },
		{ KeyboardKey.N, ImGuiKey.N },
		{ KeyboardKey.O, ImGuiKey.O },
		{ KeyboardKey.P, ImGuiKey.P },
		{ KeyboardKey.Q, ImGuiKey.Q },
		{ KeyboardKey.R, ImGuiKey.R },
		{ KeyboardKey.S, ImGuiKey.S },
		{ KeyboardKey.T, ImGuiKey.T },
		{ KeyboardKey.U, ImGuiKey.U },
		{ KeyboardKey.V, ImGuiKey.V },
		{ KeyboardKey.W, ImGuiKey.W },
		{ KeyboardKey.X, ImGuiKey.X },
		{ KeyboardKey.Y, ImGuiKey.Y },
		{ KeyboardKey.Z, ImGuiKey.Z },

		{ KeyboardKey.F1, ImGuiKey.F1 },
		{ KeyboardKey.F2, ImGuiKey.F2 },
		{ KeyboardKey.F3, ImGuiKey.F3 },
		{ KeyboardKey.F4, ImGuiKey.F4 },
		{ KeyboardKey.F5, ImGuiKey.F5 },
		{ KeyboardKey.F6, ImGuiKey.F6 },
		{ KeyboardKey.F7, ImGuiKey.F7 },
		{ KeyboardKey.F8, ImGuiKey.F8 },
		{ KeyboardKey.F9, ImGuiKey.F9 },
		{ KeyboardKey.F10, ImGuiKey.F10 },
		{ KeyboardKey.F11, ImGuiKey.F11 },
		{ KeyboardKey.F12, ImGuiKey.F12 },

		{ KeyboardKey.Apostrophe, ImGuiKey.Apostrophe },
		{ KeyboardKey.Comma, ImGuiKey.Comma },
		{ KeyboardKey.Minus, ImGuiKey.Minus },
		{ KeyboardKey.Period, ImGuiKey.Period },
		{ KeyboardKey.Slash, ImGuiKey.Slash },
		{ KeyboardKey.Semicolon, ImGuiKey.Semicolon },
		{ KeyboardKey.Equal, ImGuiKey.Equal },
		{ KeyboardKey.LeftBracket, ImGuiKey.LeftBracket },
		{ KeyboardKey.Backslash, ImGuiKey.Backslash },
		{ KeyboardKey.RightBracket, ImGuiKey.RightBracket },
		{ KeyboardKey.Grave, ImGuiKey.GraveAccent },
		{ KeyboardKey.CapsLock, ImGuiKey.CapsLock },
		{ KeyboardKey.ScrollLock, ImGuiKey.ScrollLock },
		{ KeyboardKey.NumLock, ImGuiKey.NumLock },
		{ KeyboardKey.PrintScreen, ImGuiKey.PrintScreen },
		{ KeyboardKey.Pause, ImGuiKey.Pause },

		{ KeyboardKey.Kp0, ImGuiKey.Keypad0 },
		{ KeyboardKey.Kp1, ImGuiKey.Keypad1 },
		{ KeyboardKey.Kp2, ImGuiKey.Keypad2 },
		{ KeyboardKey.Kp3, ImGuiKey.Keypad3 },
		{ KeyboardKey.Kp4, ImGuiKey.Keypad4 },
		{ KeyboardKey.Kp5, ImGuiKey.Keypad5 },
		{ KeyboardKey.Kp6, ImGuiKey.Keypad6 },
		{ KeyboardKey.Kp7, ImGuiKey.Keypad7 },
		{ KeyboardKey.Kp8, ImGuiKey.Keypad8 },
		{ KeyboardKey.Kp9, ImGuiKey.Keypad9 },
		{ KeyboardKey.KpDecimal, ImGuiKey.KeypadDecimal },
		{ KeyboardKey.KpDivide, ImGuiKey.KeypadDivide },
		{ KeyboardKey.KpMultiply, ImGuiKey.KeypadMultiply },
		{ KeyboardKey.KpSubtract, ImGuiKey.KeypadSubtract },
		{ KeyboardKey.KpAdd, ImGuiKey.KeypadAdd },
		{ KeyboardKey.KpEnter, ImGuiKey.KeypadEnter },
		{ KeyboardKey.KpEqual, ImGuiKey.KeypadEqual },
	};
	private readonly static Dictionary<GamepadButton, ImGuiKey> _gamepadButtonMap = new()
	{
		{ GamepadButton.MiddleRight, ImGuiKey.GamepadStart },
		{ GamepadButton.MiddleLeft, ImGuiKey.GamepadBack },
    
		{ GamepadButton.RightFaceLeft, ImGuiKey.GamepadFaceLeft },
		{ GamepadButton.RightFaceRight, ImGuiKey.GamepadFaceRight },
		{ GamepadButton.RightFaceUp, ImGuiKey.GamepadFaceUp },
		{ GamepadButton.RightFaceDown, ImGuiKey.GamepadFaceDown },
    
		{ GamepadButton.LeftFaceLeft, ImGuiKey.GamepadDpadLeft },
		{ GamepadButton.LeftFaceRight, ImGuiKey.GamepadDpadRight },
		{ GamepadButton.LeftFaceUp, ImGuiKey.GamepadDpadUp },
		{ GamepadButton.LeftFaceDown, ImGuiKey.GamepadDpadDown },
    
		{ GamepadButton.LeftTrigger1, ImGuiKey.GamepadL1 },
		{ GamepadButton.RightTrigger1, ImGuiKey.GamepadR1 },
		{ GamepadButton.LeftTrigger2, ImGuiKey.GamepadL2 },
		{ GamepadButton.RightTrigger2, ImGuiKey.GamepadR2 },
	};
	private readonly static Dictionary<(GamepadAxis axis, bool isPositive), ImGuiKey> _gamepadAxisMap = new()
	{
		{ (GamepadAxis.LeftX, false), ImGuiKey.GamepadLStickLeft },
		{ (GamepadAxis.LeftX, true), ImGuiKey.GamepadLStickRight },
		{ (GamepadAxis.LeftY, false), ImGuiKey.GamepadLStickUp },
		{ (GamepadAxis.LeftY, true), ImGuiKey.GamepadLStickDown },
    
		{ (GamepadAxis.RightX, false), ImGuiKey.GamepadRStickLeft },
		{ (GamepadAxis.RightX, true), ImGuiKey.GamepadRStickRight },
		{ (GamepadAxis.RightY, false), ImGuiKey.GamepadRStickUp },
		{ (GamepadAxis.RightY, true), ImGuiKey.GamepadRStickDown },
	};
	private readonly static Dictionary<MouseButton, ImGuiKey> _mouseButtonMap = new()
	{
		{ MouseButton.Left, ImGuiKey.MouseLeft },
		{ MouseButton.Right, ImGuiKey.MouseRight },
		{ MouseButton.Middle, ImGuiKey.MouseMiddle },
		{ MouseButton.Side, ImGuiKey.MouseX1 },
		{ MouseButton.Extra, ImGuiKey.MouseX2 },
	};

	private List<int> _pads = [];

	private void InitInput()
	{
		EventServices.Instance.Subscribe<GamepadRegisterEvent>(@event => { _pads.Add(@event.GamepadId); });
		EventServices.Instance.Subscribe<GamepadUnregisterEvent>(@event => { _pads.Remove(@event.GamepadId); });
	}

	private void InputCheck()
	{
		UpdateDisplaySize();
		foreach (var key in KeyboardInputs.GetPressedKeys())
		{
			_io.AddKeyEvent(_keyboardMap[key], true);
		}
		_io.KeyCtrl = KeyboardInputs.IsKeyPressed(KeyboardKey.LeftControl) || 
		              KeyboardInputs.IsKeyPressed(KeyboardKey.RightControl);
              
		_io.KeyShift = KeyboardInputs.IsKeyPressed(KeyboardKey.LeftShift) || 
		               KeyboardInputs.IsKeyPressed(KeyboardKey.RightShift);
               
		_io.KeyAlt = KeyboardInputs.IsKeyPressed(KeyboardKey.LeftAlt) || 
		             KeyboardInputs.IsKeyPressed(KeyboardKey.RightAlt);
             
		_io.KeySuper = KeyboardInputs.IsKeyPressed(KeyboardKey.LeftSuper) || 
		               KeyboardInputs.IsKeyPressed(KeyboardKey.RightSuper);
		_io.MousePos = Raylib.GetMousePosition();
		foreach (var key in _mouseButtonMap)
		{
			var pressed = MouseButtonInputs.IsButtonPressed(key.Key);
			var buttonIndex = (int)key.Key;
			_io.AddMouseButtonEvent(buttonIndex, pressed);
		}
		var wheel = Raylib.GetMouseWheelMoveV();
		_io.AddMouseWheelEvent(wheel.X, wheel.Y);
		int pressedChar;
		foreach (var pad in _pads)
		{
			CheckPad(pad);
		}
		while ((pressedChar = Raylib.GetCharPressed()) != 0)
		{
			_io.AddInputCharacter((uint)pressedChar);
		}
	}
	private void UpdateDisplaySize()
	{
		_io.DisplaySize = new System.Numerics.Vector2(
			Raylib.GetScreenWidth(),
			Raylib.GetScreenHeight()
		);
		_io.DisplayFramebufferScale = System.Numerics.Vector2.One;
	}

	private void CheckPad(int id)
	{
		foreach (var key in _gamepadButtonMap)
		{
			var pressed = GamepadInputs.IsButtonPressed(id, key.Key);
			_io.AddKeyEvent(key.Value, pressed);
		}
		foreach (var axis in _gamepadAxisMap)
		{
			var value = GamepadInputs.GetCurrentAxisValue(id, axis.Key.axis);
			var pressed = axis.Key.isPositive ? value > 0 : value < 0;
			_io.AddKeyEvent(axis.Value, pressed);
		}
	}
}