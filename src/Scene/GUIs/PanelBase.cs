namespace LibAurora.Scene.GUIs;

/// <summary>
/// A container that draws a styled panel background behind its children.
/// Subclasses implement <see cref="DrawPanel"/> to define the visual appearance.
/// </summary>
public abstract class PanelBase : GuiContainer
{
	/// <inheritdoc />
	protected override void DrawBeforeChildren(StyleRenderer renderer)
	{
		base.DrawBeforeChildren(renderer);
		DrawPanel(renderer);
	}

	/// <summary>Draws the panel background. Called after the clip is pushed but before children are drawn.</summary>
	protected abstract void DrawPanel(StyleRenderer renderer);
}