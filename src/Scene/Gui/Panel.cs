namespace LibAurora.Scene.Gui;

/// <summary>
/// A basic container with an optional background <see cref="Style"/>.
/// The background is drawn before all child elements.
/// </summary>
public class Panel : GuiContainer
{
	/// <summary>The style of panel. It will be rendered before rendering children. </summary>
	public Style? BackgroundStyle { get; set; }
	/// <summary>Draws the background style before rendering children.</summary>
	protected override void DrawBeforeChildren(StyleRenderer renderer)
	{
		if (BackgroundStyle is not null) renderer.Draw(BackgroundStyle, Bounds);
		base.DrawBeforeChildren(renderer);
	}
}