using System.Numerics;
using Veldrid;
namespace LibAurora.Input;

public interface IInput
{
	public bool IsKeyDown(Key key);
	public bool IsKeyPressed(Key key);
	public bool IsMouseButtonDown(MouseButton button);
	public Vector2 GetMousePosition();
}