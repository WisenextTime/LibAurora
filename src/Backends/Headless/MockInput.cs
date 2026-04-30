using System.Numerics;
using LibAurora.Input;
using Veldrid;
namespace LibAurora.Backends.Headless;

/// <summary>
/// Mock input implementation. All queries return default/neutral values.
/// Used in headless or testing scenarios.
/// </summary>
public class MockInput : IInput
{
	/// <inheritdoc />
	public bool IsKeyDown(Key key) => false;

	/// <inheritdoc />
	public bool IsKeyPressed(Key key) => false;

	/// <inheritdoc />
	public bool IsMouseButtonDown(MouseButton button) => false;

	/// <inheritdoc />
	public Vector2 GetMousePosition() => Vector2.Zero;

	/// <inheritdoc />
	public float GetMouseWheelDelta() => 0;
}