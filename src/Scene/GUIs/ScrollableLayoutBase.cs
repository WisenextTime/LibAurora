using System;
using System.Numerics;
using LibAurora.Core;
namespace LibAurora.Scene.GUIs;

/// <summary>
/// A container with scrollable content that extends beyond its viewport.
/// Supports mouse wheel scrolling and draws scrollbar indicators when content overflows.
/// Subclasses implement <see cref="DrawScrollbar"/> to customize scrollbar appearance.
/// </summary>
public abstract class ScrollableLayoutBase : GuiContainer
{
	private Vector2 _scrollOffset;

	/// <summary>The current scroll offset in pixels. Clamped to valid content bounds.</summary>
	public Vector2 ScrollOffset
	{
		get => _scrollOffset;
		set
		{
			var old = _scrollOffset;
			_scrollOffset = ClampOffset(value);
			if (old != _scrollOffset)
				Events.Raise(new Events.ValueChangedEvent<Vector2>(this, old, _scrollOffset));
		}
	}

	/// <summary>The total size of the content area. Set by subclasses during layout.</summary>
	public (uint Width, uint Height) ContentSize { get; protected set; }

	/// <summary>Pixels scrolled per mouse wheel notch. Default is 30.</summary>
	public float ScrollSpeed { get; set; } = 30f;

	/// <inheritdoc />
	public override bool HandleInput()
	{
		if (!Visible || !Enabled || Input == null) return false;
		var wheelDelta = Input.GetMouseWheelDelta();
		if (wheelDelta != 0)
			ScrollBy(new Vector2(0, -wheelDelta * ScrollSpeed));
		return base.HandleInput() || wheelDelta != 0;
	}

	/// <summary>Scrolls the content by the given delta in pixels.</summary>
	public void ScrollBy(Vector2 delta)
	{
		ScrollOffset += delta;
	}

	/// <inheritdoc />
	protected override void DrawAfterChildren(StyleRenderer renderer)
	{
		if (Visible)
		{
			if (ContentSize.Height > Size.Y || ContentSize.Width > Size.X)
			{
				DrawScrollbar(renderer,
					Size.X, Size.Y,
					ContentSize.Width, ContentSize.Height,
					_scrollOffset.X, _scrollOffset.Y);
			}
		}
		base.DrawAfterChildren(renderer);
	}

	/// <summary>Draws the scrollbar indicators. Called after children are drawn but before the clip is popped.</summary>
	protected abstract void DrawScrollbar(StyleRenderer renderer,
		uint viewportWidth, uint viewportHeight,
		uint contentWidth, uint contentHeight,
		float offsetX, float offsetY);

	private Vector2 ClampOffset(Vector2 offset)
	{
		var maxX = Math.Max(0, ContentSize.Width - Size.X);
		var maxY = Math.Max(0, ContentSize.Height - Size.Y);
		return new Vector2(
			Math.Clamp(offset.X, 0, maxX),
			Math.Clamp(offset.Y, 0, maxY));
	}
}