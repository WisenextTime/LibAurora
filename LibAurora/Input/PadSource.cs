using System;
using Raylib_cs;
namespace LibAurora.Input;

public class PadButtonSource(int padId,GamepadButton button) : InputSource
{

	public override bool Pressed()
		=> Raylib.IsGamepadButtonPressed(padId, button);
	public override bool Down()
		=> Raylib.IsGamepadButtonDown(padId, button);
	public override bool Released()
		=> Raylib.IsGamepadButtonReleased(padId, button);
	public override bool Up()
		=> Raylib.IsGamepadButtonUp(padId, button);
	public override float Strength()
		=> Raylib.IsGamepadButtonDown(padId, button);
}

public class PadAxisSource(int padId, GamepadAxis axis, float deadZone) : InputSource
{
	public override bool Pressed()
		=> Math.Abs(Raylib.GetGamepadAxisMovement(padId, axis)) > deadZone;
	public override bool Down()
		=> Math.Abs(Raylib.GetGamepadAxisMovement(padId, axis)) > deadZone;
	public override bool Released()
		=> Math.Abs(Raylib.GetGamepadAxisMovement(padId, axis)) < deadZone;
	public override bool Up()
		=> Math.Abs(Raylib.GetGamepadAxisMovement(padId, axis)) < deadZone;
	public override float Strength()
		=> Math.Abs(Raylib.GetGamepadAxisMovement(padId, axis)) > deadZone? Raylib.GetGamepadAxisMovement(padId, axis):0;
}