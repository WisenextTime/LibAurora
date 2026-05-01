using System.Collections.Generic;
using LibAurora.Core;
using LibAurora.Input;
using LibAurora.Scene.GUIs;
using Veldrid;
namespace LibAurora.Scene;

/// <summary>
/// A GUI scene that holds a <see cref="StyleRenderer"/> and a tree of <see cref="GuiElement"/>s.
/// Dispatches update, draw, and input events to the root element.
/// Forwards editing keys and SDL2 text input to the focused <see cref="TextBoxBase"/>.
/// Supports key repeat for editing keys (BackSpace, Delete, arrows) after a hold delay.
/// </summary>
public class Gui(StyleRenderer renderer) : Scene
{
	private const double RepeatDelay = 0.4;
	private const double RepeatInterval = 0.1;
	private readonly static Key[] _editingKeys =
	[
		Key.BackSpace, Key.Delete, Key.Left, Key.Right, Key.Home, Key.End,
		Key.Enter, Key.Up, Key.Down
	];
	private readonly static Dictionary<Key, (double PressTime, double LastRepeat)> _keyRepeat = [];
	private double _totalTime;

	/// <summary>The root element of the GUI tree.</summary>
	public GuiElement? Root { get; set; }

	/// <summary>When false, input handling is skipped for this frame.</summary>
	public bool CanHandleEvent { get; set; } = true;

	/// <summary>Initializes the static input reference shared by all GUI elements, and subscribes to text input events.</summary>
	public static void InitInput(IInput input)
	{
		GuiElement.Input = input;
		Events.Subscribe<Events.TextInputEvent>(OnTextInput);
	}

	/// <inheritdoc />
	public override bool HandleInput() => CanHandleEvent && (Root?.HandleInput() ?? false);

	/// <inheritdoc />
	public override void Update(double delta)
	{
		_totalTime += delta;
		Root?.Update(delta);
		if (GuiElement.FocusedElement is not TextBoxBase tb || GuiElement.Input is not { } input)
		{
			_keyRepeat.Clear();
			return;
		}
		foreach (var key in _editingKeys)
		{
			if (input.IsKeyPressed(key))
			{
				tb.HandleKeyInput(key);
				_keyRepeat[key] = (_totalTime, _totalTime);
			}
			else if (input.IsKeyDown(key) && _keyRepeat.TryGetValue(key, out var rt))
			{
				var elapsed = _totalTime - rt.PressTime;
				if (elapsed >= RepeatDelay && _totalTime - rt.LastRepeat >= RepeatInterval)
				{
					tb.HandleKeyInput(key);
					_keyRepeat[key] = (rt.PressTime, _totalTime);
				}
			}
			else if (!input.IsKeyDown(key))
			{
				_keyRepeat.Remove(key);
			}
		}
		var (cX, cY) = tb.AbsolutePosition;
		input.SetTextInputRect(cX, cY, tb.Size.X, tb.Size.Y);
	}

	/// <inheritdoc />
	public override void Draw(double alpha)
	{
		Root?.Draw(renderer);
	}

	private static void OnTextInput(Events.TextInputEvent e)
	{
		if (GuiElement.FocusedElement is TextBoxBase tb)
			tb.HandleTextInput(e.Character);
	}
}