using System;
using System.Numerics;
using LibAurora.Core;
using Veldrid;
namespace LibAurora.Scene.GUIs;

/// <summary>
/// Base class for single-line text input controls. Manages text editing,
/// caret positioning with blink animation, and keyboard/mouse input.
/// Subclasses implement rendering via <see cref="DrawTextBox"/> and <see cref="DrawCaret"/>.
/// </summary>
public abstract class TextBoxBase : GuiElement
{
	const double CaretBlinkInterval = 0.5;
	private uint _caretPosition;
	private double _caretTimer;
	private bool _caretVisible = true;
	private string _text = string.Empty;

	/// <summary>
	/// The current text content. Setting this property raises
	/// <see cref="OnTextChanged"/> and a <see cref="Events.ValueChangedEvent{T}"/>.
	/// </summary>
	public string Text
	{
		get => _text;
		set
		{
			if (_text == value) return;
			var old = _text;
			_text = value;
			_caretPosition = (uint)Math.Min(_caretPosition, _text.Length);
			OnTextChanged(old, _text);
			Events.Raise(new Events.ValueChangedEvent<string>(this, old, _text));
		}
	}

	/// <summary>The current character index of the caret within the text.</summary>
	public uint CaretPosition
	{
		get => _caretPosition;
		set => _caretPosition = (uint)Math.Min(value, _text.Length);
	}

	/// <summary>Maximum allowed length of the text. Default is unlimited.</summary>
	public uint MaxLength { get; set; } = uint.MaxValue;

	/// <inheritdoc />
	public override void Update(double delta)
	{
		if (!Visible || !Enabled) return;
		if (Focused)
		{
			_caretTimer += delta;
			if (_caretTimer >= CaretBlinkInterval)
			{
				_caretTimer -= CaretBlinkInterval;
				_caretVisible = !_caretVisible;
			}
		}
	}

	/// <inheritdoc />
	public override bool HandleInput()
	{
		if (!Visible || !Enabled || Input == null) return false;
		var mousePos = Input.GetMousePosition();
		if (!Input.IsMouseButtonDown(MouseButton.Left))
			return Focused;

		if (IsPointInside(mousePos))
		{
			RequestFocus(this);
			UpdateCaretFromMouse(mousePos);
			return true;
		}

		if (Focused) ReleaseFocus();
		_caretVisible = false;
		return false;
	}

	/// <summary>Inserts a character at the current caret position, if focused and below max length.</summary>
	public void HandleTextInput(char character)
	{
		if (!Focused || _text.Length >= MaxLength) return;
		var old = _text;
		_text = _text.Insert((int)_caretPosition, character.ToString());
		_caretPosition++;
		OnTextChanged(old, _text);
		Events.Raise(new Events.ValueChangedEvent<string>(this, old, _text));
	}

	/// <summary>Handles keyboard navigation and editing keys (backspace, delete, arrows, home, end).</summary>
	public virtual void HandleKeyInput(Key key)
	{
		if (!Focused) return;
		var old = _text;
		switch (key)
		{
			case Key.BackSpace when _caretPosition > 0:
				_text = _text.Remove((int)_caretPosition - 1, 1);
				_caretPosition--;
				break;
			case Key.Delete when _caretPosition < _text.Length:
				_text = _text.Remove((int)_caretPosition, 1);
				break;
			case Key.Left:
				if (_caretPosition > 0) _caretPosition--;
				return;
			case Key.Right:
				if (_caretPosition < _text.Length) _caretPosition++;
				return;
			case Key.Home:
				_caretPosition = 0;
				return;
			case Key.End:
				_caretPosition = (uint)_text.Length;
				return;
			default:
				return;
		}
		if (old != _text)
		{
			OnTextChanged(old, _text);
			Events.Raise(new Events.ValueChangedEvent<string>(this, old, _text));
		}
	}

	/// <inheritdoc />
	public override void Draw(StyleRenderer renderer)
	{
		base.Draw(renderer);
		if (!Visible) return;
		DrawTextBox(renderer);
		if (Focused && _caretVisible)
			DrawCaret(renderer, _caretPosition);
	}

	/// <summary>Draws the text box background and text content.</summary>
	protected abstract void DrawTextBox(StyleRenderer renderer);

	/// <summary>Draws the blinking text caret at the specified character position.</summary>
	protected abstract void DrawCaret(StyleRenderer renderer, uint caretPosition);

	/// <summary>Called when the mouse clicks inside the text box to reposition the caret.</summary>
	protected virtual void UpdateCaretFromMouse(Vector2 mousePosition) { }

	/// <summary>Called whenever the text content changes, before the event is raised.</summary>
	protected virtual void OnTextChanged(string oldText, string newText) { }

	/// <inheritdoc />
	protected override void OnFocusGained()
	{
		_caretVisible = true;
		_caretTimer = 0;
	}

	/// <inheritdoc />
	protected override void OnFocusLost()
	{
		_caretVisible = false;
	}
}