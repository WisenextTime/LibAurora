using System.Numerics;
using LibAurora.Input;
using Veldrid;
namespace LibAurora.Backends.Headless;

public class MockInput : IInput
{
	public bool IsKeyDown(Key key) => false;
	public bool IsKeyPressed(Key key) => false;
	public bool IsMouseButtonDown(MouseButton button) => false;
	public Vector2 GetMousePosition() => Vector2.Zero;
}