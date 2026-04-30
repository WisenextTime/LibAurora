using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LibAurora.Input;
using LibAurora.Scene.GUIs;
namespace LibAurora.Scene;

/// <summary>
/// Abstract base for all GUI elements. Manages position, size, visibility,
/// parent-child hierarchy, and provides the global focus system.
/// </summary>
public abstract class GuiElement
{

	/// <summary>Position of the element relative to its parent.</summary>
	public (uint X, uint Y) Position = (0, 0);

	/// <summary>Size of the element in pixels.</summary>
	public (uint X, uint Y) Size = (0, 0);
	/// <summary>Static input source shared across all GUI elements.</summary>
	public static IInput? Input { get; set; }

	/// <summary>The currently focused element, or null if none.</summary>
	public static GuiElement? FocusedElement { get; private set; }

	/// <summary>The parent container, or null if this is the root element.</summary>
	public GuiContainer? Parent { get; internal set; }

	/// <summary>When false, the element is invisible and skips drawing.</summary>
	public bool Visible { get; set; } = true;

	/// <summary>When false, the element ignores all input.</summary>
	public bool Enabled { get; set; } = true;

	/// <summary>True if this element currently holds focus.</summary>
	public bool Focused => FocusedElement == this;

	/// <summary>
	/// The absolute position of this element in window coordinates,
	/// computed by walking up the parent chain and summing relative positions.
	/// </summary>
	public (uint X, uint Y) AbsolutePosition
	{
		get
		{
			var x = Position.X;
			var y = Position.Y;
			var parent = Parent;
			while (parent != null)
			{
				x += parent.Position.X;
				y += parent.Position.Y;
				parent = parent.Parent;
			}
			return (x, y);
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

	/// <summary>Requests focus for the specified element, notifying previous and new holder.</summary>
	public static void RequestFocus(GuiElement element)
	{
		if (FocusedElement == element) return;
		var previous = FocusedElement;
		FocusedElement = element;
		previous?.OnFocusLost();
		element.OnFocusGained();
	}

	/// <summary>Releases focus from the currently focused element.</summary>
	public static void ReleaseFocus()
	{
		if (FocusedElement == null) return;
		FocusedElement.OnFocusLost();
		FocusedElement = null;
	}

	/// <summary>Called when this element gains focus.</summary>
	protected virtual void OnFocusGained() { }

	/// <summary>Called when this element loses focus.</summary>
	protected virtual void OnFocusLost() { }
}
/// <summary>
/// A GUI element that contains and manages child elements.
/// Handles layout, clipping, and input event bubbling.
/// </summary>
public abstract class GuiContainer : GuiElement
{
	private readonly List<GuiElement> _elements = [];

	/// <summary>Read-only view of child elements.</summary>
	public IReadOnlyList<GuiElement> Children => _elements;

	/// <summary>Adds a child element. Throws if the element already belongs to another parent.</summary>
	public void Add(GuiElement element)
	{
		if (element.Parent != null && element.Parent != this)
			throw new InvalidOperationException("Element already belongs to another container.");
		element.Parent = this;
		_elements.Add(element);
	}

	/// <summary>Removes a child element. Releases focus if the removed element was focused.</summary>
	public void Remove(GuiElement element)
	{
		_elements.Remove(element);
		element.Parent = null;
		if (FocusedElement == element) ReleaseFocus();
	}

	/// <inheritdoc />
	public override void Update(double delta)
	{
		UpdateLayout();
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
		renderer.ResetClip();
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