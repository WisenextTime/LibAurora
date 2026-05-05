using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using LibAurora.Scene.Gui;
namespace LibAurora.Scene;

/// <summary>
/// Abstract base for all GUI elements. Manages position, size, visibility,
/// parent-child hierarchy, and provides the global focus system.
/// </summary>
public abstract class GuiElement
{
	/// <summary>Position of the element relative to its parent.</summary>
	public virtual (uint X, uint Y) Position { get; set; } = (0, 0);

	/// <summary>Size of the element in pixels.</summary>
	public virtual (uint X, uint Y) Size { get; set; } = (0, 0);

	/// <summary>The parent container, or null if this is the root element.</summary>
	public GuiElement? Parent { get; set; }

	/// <summary>When false, the element is invisible and skips drawing.</summary>
	public bool Visible { get; set; } = true;

	/// <summary>When false, the element ignores all input and update.</summary>
	public bool Enabled { get; set; } = true;

	/// <summary>When true, the element has unique behavior.</summary>
	public bool Focused { get; set; } = false;

	/// <summary>
	/// The absolute position of this element in window coordinates,
	/// computed by walking up the parent chain and summing relative positions.
	/// </summary>
	public (uint X, uint Y) AbsolutePosition
	{
		get
		{
			var x = Position.X + Parent?.AbsolutePosition.X ?? 0;
			var y = Position.Y + Parent?.AbsolutePosition.Y ?? 0;
			return (x, y);
		}
	}

	/// <summary>Get the bounds of this element </summary>
	public RectangleF Bounds
	{
		get
		{
			var (x, y) = AbsolutePosition;
			return new RectangleF(x, y, Size.X, Size.Y);
		}
	}

	/// <summary>Called once per frame with delta time in seconds.</summary>
	public virtual void Update(double delta) { }

	/// <summary>Draws the element using the given renderer.</summary>
	public virtual void Draw(StyleRenderer renderer) { }

	/// <summary>Handles input. Returns true if the input event was consumed.</summary>
	public virtual bool HandleInput() => false;

	/// <summary>Checks whether a point in absolute window coordinates is inside this element.</summary>
	public bool IsPointInside(Vector2 point)
	{
		var (ax, ay) = AbsolutePosition;
		return point.X >= ax &&
		       point.X <= ax + Size.X &&
		       point.Y >= ay &&
		       point.Y <= ay + Size.Y;
	}
}
/// <summary>
/// A GUI element that contains and manages child elements.
/// Handles layout, clipping, and input event bubbling.
/// </summary>
public abstract class GuiContainer : GuiElement
{
	private readonly List<GuiElement> _elements = [];

	public override (uint X, uint Y) Size
	{
		get;
		set
		{
			field = value;
			UpdateLayout();
		}
	}

	/// <summary>Read-only view of child elements.</summary>
	public IReadOnlyList<GuiElement> Children => _elements;

	/// <summary>Adds a child element. Throws if the element already belongs to another parent.</summary>
	public void Add(GuiElement element)
	{
		if (element.Parent != null && element.Parent != this)
			throw new InvalidOperationException("Element already belongs to another container.");
		element.Parent = this;
		_elements.Add(element);
		UpdateLayout();
	}

	/// <summary>Removes a child element. Releases focus if the removed element was focused.</summary>
	public void Remove(GuiElement element)
	{
		_elements.Remove(element);
		element.Parent = null;
		UpdateLayout();
	}

	/// <inheritdoc />
	public override void Update(double delta)
	{
		foreach (var element in _elements)
			element.Update(delta);
	}

	/// <summary>Called before children are updated to recalculate layout.</summary>
	protected virtual void UpdateLayout() { }

	/// <inheritdoc />
	public override void Draw(StyleRenderer renderer)
	{
		base.Draw(renderer);
		DrawBeforeChildren(renderer);
		foreach (var element in _elements)
			element.Draw(renderer);
		DrawAfterChildren(renderer);
	}

	/// <summary>Called before drawing children. Default pushes a clip region matching this container.</summary>
	protected virtual void DrawBeforeChildren(StyleRenderer renderer)
	{
		renderer.PushClip(Position.X, Position.Y, Size.X, Size.Y);
	}

	/// <summary>Called after drawing children. Default pops the clip region.</summary>
	protected virtual void DrawAfterChildren(StyleRenderer renderer)
	{
		renderer.PopClip();
	}

	/// <inheritdoc />
	/// <remarks>Iterates children in order; stops at the first child that consumes input.</remarks>
	public override bool HandleInput()
	{
		var inputHandled = false;
		foreach (var element in _elements.TakeWhile(_ => !inputHandled))
			inputHandled = element.HandleInput();
		return inputHandled;
	}
}