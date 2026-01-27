using Raylib_cs;
namespace LibAurora.Input;

public class KeySource(KeyboardKey key) : InputSource
{

	public override bool Pressed() => Raylib.IsKeyPressed(key);
	public override bool Down() => Raylib.IsKeyDown(key);
	public override bool Released() => Raylib.IsKeyReleased(key);
	public override bool Up() => Raylib.IsKeyUp(key);
	public override float Strength() => Raylib.IsKeyDown(key);
}