using LibAurora.Input;
using LibAurora.MathUtils;
using Myra.Graphics2D.UI;
using Myra.Platform;
using Veldrid;
using Point = System.Drawing.Point;
namespace LibAurora.Graphics.Myra;

/// <summary>
/// Bridges <see cref="IInput"/> and viewport info to the <see cref="IMyraPlatform"/> interface.
/// Creates the <see cref="MyraRenderer"/> internally and handles mouse/keyboard/touch input mapping.
/// </summary>
public class MyraPlatform(IGraphics graphics, IInput input, Shader[] shaders) : IMyraPlatform
{
	/// <summary>Builds a Myra <see cref="MouseInfo"/> from the current <see cref="IInput"/> state.</summary>
	public MouseInfo GetMouseInfo()
	{
		var info = new MouseInfo
		{
			Position = input.GetMousePosition().ToPoint(),
			IsLeftButtonDown = input.IsMouseButtonDown(MouseButton.Left),
			IsRightButtonDown = input.IsMouseButtonDown(MouseButton.Right),
			IsMiddleButtonDown = input.IsMouseButtonDown(MouseButton.Middle),
			Wheel = input.GetMouseWheelDelta(),
		};
		return info;
	}
	/// <summary>Fills the bool array with current keyboard state via <see cref="MyraKeyMapUtils.FillKeysDown"/>.</summary>
	public void SetKeysDown(bool[] keys)
	{
		MyraKeyMapUtils.FillKeysDown(keys, input);
	}
	/// <summary>Sets the mouse cursor type. Not yet implemented.</summary>
	public void SetMouseCursorType(MouseCursorType mouseCursorType)
	{

	}
	/// <summary>Returns an empty <see cref="TouchCollection"/>. Mobile touch is not yet implemented.</summary>
	public TouchCollection GetTouchState()
	{
		var touchCollection = new TouchCollection
		{
			//I will implement this when I Implement mobile support.
		};
		return touchCollection;
	}
	/// <summary>The current viewport dimensions in pixels.</summary>
	public Point ViewSize => new((int)graphics.ViewportWidth, (int)graphics.ViewportHeight);

	/// <summary>The Veldrid-backed <see cref="MyraRenderer"/> instance.</summary>
	public IMyraRenderer Renderer { get; } = new MyraRenderer(graphics, shaders);
}