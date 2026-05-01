using System;
using Veldrid;
namespace LibAurora.Scene.GUIs;

/// <summary>
/// Base class for multiline text input controls. Extends <see cref="TextBoxBase"/>
/// with line-aware navigation (Enter inserts newlines, Up/Down move between lines,
/// Home/End navigate within the current line).
/// </summary>
public abstract class MultilineTextBoxBase : TextBoxBase
{
	/// <inheritdoc />
	public override void HandleKeyInput(Key key)
	{
		if (!Focused) return;
		switch (key)
		{
			case Key.Enter:
				HandleTextInput('\n');
				break;
			case Key.Up:
				MoveCaretLine(-1);
				break;
			case Key.Down:
				MoveCaretLine(1);
				break;
			case Key.Home:
				CaretPosition = GetLineStart(CaretPosition);
				break;
			case Key.End:
				CaretPosition = GetLineEnd(CaretPosition);
				break;
			default:
				base.HandleKeyInput(key);
				break;
		}
	}

	private void MoveCaretLine(int direction)
	{
		var lineStart = GetLineStart(CaretPosition);
		var column = (int)(CaretPosition - lineStart);

		uint targetLineStart;
		if (direction < 0)
		{
			if (lineStart == 0) return;
			targetLineStart = GetLineStart(lineStart - 1);
		}
		else
		{
			var lineEnd = GetLineEnd(CaretPosition);
			if (lineEnd >= Text.Length) return;
			targetLineStart = lineEnd + 1;
		}

		var targetLineEnd = GetLineEnd(targetLineStart);
		var targetLineLength = targetLineEnd - targetLineStart;
		CaretPosition = targetLineStart + (uint)Math.Min(column, (int)targetLineLength);
	}

	private uint GetLineStart(uint position)
	{
		var pos = (int)position - 1;
		while (pos >= 0 && Text[pos] != '\n') pos--;
		return (uint)(pos + 1);
	}

	private uint GetLineEnd(uint position)
	{
		var pos = (int)position;
		while (pos < Text.Length && Text[pos] != '\n') pos++;
		return (uint)pos;
	}
}