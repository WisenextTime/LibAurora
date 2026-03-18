using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LibAurora.Graphics;
using LibAurora.Input;
using Veldrid;
namespace LibAurora.UI;

public class UIScene(IGraphics graphics, IInput input)
{
	public readonly List<UIElement> Root = [];
	private bool _currentMouseState;
	private bool _lastMouseState;
	public void Update()
	{
		foreach (var child in Root)
			child.Update();

		var topUI = Root.LastOrDefault();
		if (topUI?.Enabled != true)
			return;

		var position = input.GetMousePosition();

		_lastMouseState = _currentMouseState;
		_currentMouseState = input.IsMouseButtonDown(MouseButton.Left);

		HandleUIEvents(topUI, position);
	}

	private void HandleUIEvents(UIElement ui, Vector2 position)
	{
		ui.OnMouseMove(position);

		switch (_currentMouseState)
		{
			case true when !_lastMouseState:
				ui.OnMouseDown(position);
				break;
			case false when _lastMouseState:
				ui.OnMouseUp(position);
				break;
		}

	}
	public void Render()
	{
		foreach (var child in Root) child.Render(graphics.CommandList);
	}
	public void PushUI(UIElement child) => Root.Add(child);
	public void PopUI() => Root.RemoveAt(Root.Count - 1);
	public void ClearUI()
	{
		Root.Clear();
	}
}

// ReSharper disable once FunctionRecursiveOnAllPaths
public class UIElement
{
	public bool Visible { get; set; } = true;
	public bool Enabled { get; set; } = true;

	public float X { get; set; }
	public float Y { get; set; }
	public float Width { get; set; }
	public float Height { get; set; }

	public float PaddingTop { get; set; }
	public float PaddingBottom { get; set; }
	public float PaddingLeft { get; set; }
	public float PaddingRight { get; set; }

	public float MarginTop { get; set; }
	public float MarginBottom { get; set; }
	public float MarginLeft { get; set; }
	public float MarginRight { get; set; }
	public UIContainer? Parent { get; set; }
	public float AbsoluteX => (Parent?.AbsoluteX ?? 0) + X + (Parent?.PaddingLeft ?? 0);
	public float AbsoluteY => (Parent?.AbsoluteY ?? 0) + Y + (Parent?.PaddingTop ?? 0);
	public bool HitTest(Vector2 point)
	{
		if (!Visible || !Enabled) return false;
		var hitX = point.X >= AbsoluteX - MarginLeft &&
		           point.X <= AbsoluteX + Width + MarginRight;
		var hitY = point.Y >= AbsoluteY - MarginTop &&
		           point.Y <= AbsoluteY + Height + MarginBottom;

		return hitX && hitY;
	}
	public virtual void Render(CommandList cl) { }
	public virtual void Update() { }
	public virtual void Initialize() { }
	public virtual bool OnMouseDown(Vector2 mousePos) => false;
	public virtual bool OnMouseUp(Vector2 mousePos) => false;
	public virtual bool OnMouseMove(Vector2 mousePos) => false;
}
public class UIContainer : UIElement
{
	protected readonly List<UIElement> Children = new();

	protected bool LayoutDirty = true;

	public void AddChild(UIElement child)
	{
		child.Parent = this;
		Children.Add(child);
		child.Initialize();
		InvalidateLayout();
	}

	public void RemoveChild(UIElement child)
	{
		if (!Children.Remove(child)) return;
		child.Parent = null;
		InvalidateLayout();
	}

	public void ClearChildren()
	{
		foreach (var child in Children)
			child.Parent = null;
		Children.Clear();
		InvalidateLayout();
	}

	protected virtual void InvalidateLayout() => LayoutDirty = true;

	protected virtual void UpdateLayout()
	{
		if (!LayoutDirty) return;

		foreach (var child in Children)
		{
			if (child is UIContainer container)
				container.UpdateLayout();
		}

		LayoutDirty = false;
	}

	public override void Update()
	{
		UpdateLayout();
		foreach (var child in Children) child.Update();
	}

	public override void Render(CommandList renderer)
	{
		if (!Visible) return;
		foreach (var child in Children) child.Render(renderer);
	}

	public override bool OnMouseDown(Vector2 mousePos)
	{
		for (var i = Children.Count - 1; i >= 0; i--)
			if (Children[i].OnMouseDown(mousePos))
				return true;
		return false;
	}
}