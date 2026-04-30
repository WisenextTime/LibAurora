using System.Numerics;
using LibAurora.Core;
using Veldrid;
namespace LibAurora.Scene.GUIs;

/// <summary>
/// An interactive button element. Tracks hover and press states,
/// and dispatches click events. Subclasses implement visual rendering
/// for each interaction state.
/// </summary>
public abstract class ButtonBase : GuiElement
{
	private bool _isHovered;
	private bool _isPressed;

	/// <summary>True while the mouse cursor is over this button.</summary>
	public bool IsHovered => _isHovered;

	/// <summary>True while the left mouse button is held down on this button.</summary>
	public bool IsPressed => _isPressed;

	/// <inheritdoc />
	public override bool HandleInput()
	{
		if (!Visible || !Enabled || Input == null) return false;
		var mousePos = Input.GetMousePosition();
		var inside = IsPointInside(mousePos);
		var wasPressed = _isPressed;
		var isDown = Input.IsMouseButtonDown(MouseButton.Left);

		_isHovered = inside;
		if (inside && isDown)
		{
			_isPressed = true;
			return true;
		}
		if (!isDown && wasPressed)
		{
			_isPressed = false;
			if (_isHovered)
			{
				OnClicked();
				Events.Raise(new Events.ClickEvent(this, mousePos));
				return true;
			}
		}
		return _isPressed;
	}

	/// <inheritdoc />
	public override void Draw(StyleRenderer renderer)
	{
		base.Draw(renderer);
		if (!Visible) return;
		if (!Enabled)
			DrawDisabled(renderer);
		else if (_isPressed && _isHovered)
			DrawPressed(renderer);
		else if (_isHovered)
			DrawHovered(renderer);
		else
			DrawNormal(renderer);
	}

	/// <summary>Draws the button in its default idle state.</summary>
	protected abstract void DrawNormal(StyleRenderer renderer);

	/// <summary>Draws the button while the mouse hovers over it.</summary>
	protected abstract void DrawHovered(StyleRenderer renderer);

	/// <summary>Draws the button while it is being pressed.</summary>
	protected abstract void DrawPressed(StyleRenderer renderer);

	/// <summary>Draws the button in its disabled (greyed out) state.</summary>
	protected abstract void DrawDisabled(StyleRenderer renderer);

	/// <summary>Called when the button is clicked (pressed then released while hovered).</summary>
	protected virtual void OnClicked() { }
}