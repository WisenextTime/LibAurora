using LibAurora.Core;
using LibAurora.Input;
using LibAurora.Scene.GUIs;
using Veldrid;
namespace LibAurora.Scene;

/// <summary>
/// A GUI scene that holds a <see cref="StyleRenderer"/> and a tree of <see cref="GuiElement"/>s.
/// Dispatches update, draw, and input events to the root element.
/// Forwards editing keys and SDL2 text input to the focused <see cref="TextBoxBase"/>.
/// </summary>
public class Gui(StyleRenderer renderer) : Scene
{
	private static readonly Key[] _editingKeys = [Key.BackSpace, Key.Delete, Key.Left, Key.Right, Key.Home, Key.End];

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
		Root?.Update(delta);
		if (GuiElement.FocusedElement is not TextBoxBase tb || GuiElement.Input is not { } input)
			return;
		foreach (var key in _editingKeys)
		{
			if (input.IsKeyPressed(key))
				tb.HandleKeyInput(key);
		}
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