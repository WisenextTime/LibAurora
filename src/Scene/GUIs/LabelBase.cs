namespace LibAurora.Scene.GUIs;

/// <summary>
/// A non-interactive text label. Displays a single line of text.
/// Subclasses implement <see cref="DrawLabel"/> to define text rendering.
/// </summary>
public abstract class LabelBase : GuiElement
{
	/// <summary>The displayed text.</summary>
	public string Text { get; set; } = string.Empty;

	/// <inheritdoc />
	public override void Draw(StyleRenderer renderer)
	{
		base.Draw(renderer);
		if (!Visible) return;
		DrawLabel(renderer, Text);
	}

	/// <summary>Renders the label text at the element's position.</summary>
	protected abstract void DrawLabel(StyleRenderer renderer, string text);
}