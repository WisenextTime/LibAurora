using Raylib_cs;
namespace LibAurora.Input;

public abstract class InputSource
{
	public abstract bool JustPressed();
	public abstract bool Pressed();
	public abstract bool JustReleased();
	public abstract bool Released();
	
	public abstract float Strength();
}

public class KeySource(KeyboardKey key) : InputSource
{

	public override bool JustPressed() => KeyboardInputs.IsKeyJustPressed(key);
	public override bool Pressed() => KeyboardInputs.IsKeyPressed(key);
	public override bool JustReleased() => KeyboardInputs.IsKeyJustReleased(key);
	public override bool Released() => KeyboardInputs.IsKeyReleased(key);
	public override float Strength() => Pressed() ? 1f : 0f;
}
public class MouseSource(MouseButton button) : InputSource
{
	public override bool JustPressed() => MouseButtonInputs.IsButtonJustPressed(button);
	public override bool Pressed() =>  MouseButtonInputs.IsButtonPressed(button);
	public override bool JustReleased() => MouseButtonInputs.IsButtonJustReleased(button);
	public override bool Released() => MouseButtonInputs.IsButtonReleased(button);
	public override float Strength() => Pressed() ? 1f : 0f;
}
public class PadButtonSource(int padId, GamepadButton button) : InputSource
{
	public override bool JustPressed() => GamepadInputs.IsButtonJustPressed(padId, button);
	public override bool Pressed() => GamepadInputs.IsButtonPressed(padId, button);
	public override bool JustReleased() => GamepadInputs.IsButtonJustReleased(padId, button);
	public override bool Released() => GamepadInputs.IsButtonReleased(padId, button);
	public override float Strength() => Pressed() ? 1f : 0f;
}
public class PadAxisSource(int padId, GamepadAxis axis) : InputSource
{
	public override bool JustPressed() => 
		GamepadInputs.GetCurrentAxisValue(padId, axis) != 0 &&
		GamepadInputs.GetPreviousAxisValue(padId, axis) == 0;
	public override bool Pressed() => GamepadInputs.GetCurrentAxisValue(padId, axis) != 0;
	public override bool JustReleased() => 
		GamepadInputs.GetCurrentAxisValue(padId, axis) == 0 &&
		GamepadInputs.GetPreviousAxisValue(padId, axis) != 0;
	public override bool Released() => GamepadInputs.GetCurrentAxisValue(padId, axis) == 0;
	public override float Strength() => GamepadInputs.GetCurrentAxisValue(padId, axis);
}