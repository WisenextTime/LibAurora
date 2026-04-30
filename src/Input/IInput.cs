using System.Numerics;
using Veldrid;
namespace LibAurora.Input;

/// <summary>
/// Input abstraction providing keyboard, mouse, and scroll wheel state.
/// Implementations poll the underlying platform's input system.
/// </summary>
public interface IInput
{
	/// <summary>True while the specified key is held down.</summary>
	public bool IsKeyDown(Key key);

	/// <summary>True on the frame the specified key was first pressed (not a repeat).</summary>
	public bool IsKeyPressed(Key key);

	/// <summary>True while the specified mouse button is held down.</summary>
	public bool IsMouseButtonDown(MouseButton button);

	/// <summary>Returns the current mouse cursor position in window coordinates.</summary>
	public Vector2 GetMousePosition();

	/// <summary>Returns the accumulated mouse wheel delta since the last call, then resets to zero.</summary>
	public float GetMouseWheelDelta();
}