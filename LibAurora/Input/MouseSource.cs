using Raylib_cs;
namespace LibAurora.Input;

public class MouseSource(MouseButton button) : InputSource
{

	public override bool Pressed() => Raylib.IsMouseButtonPressed(button);
	public override bool Down() => Raylib.IsMouseButtonDown(button);
	public override bool Released() => Raylib.IsMouseButtonReleased(button);
	public override bool Up() => Raylib.IsMouseButtonUp(button);
	public override float Strength() => Raylib.IsMouseButtonDown(button);
}