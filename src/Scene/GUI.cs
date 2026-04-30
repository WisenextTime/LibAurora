using LibAurora.Input;
using LibAurora.Scene.GUIs;
using Veldrid;
namespace LibAurora.Scene;

/// <summary>
/// A GUI scene that holds a <see cref="StyleRenderer"/> and a tree of <see cref="GuiElement"/>s.
/// Dispatches update, draw, and input events to the root element.
/// Polls keyboard input and forwards editing keys and character input to the focused <see cref="TextBoxBase"/>.
/// </summary>
public class Gui(StyleRenderer renderer) : Scene
{
	private static readonly Key[] _editingKeys = [Key.BackSpace, Key.Delete, Key.Left, Key.Right, Key.Home, Key.End];

	private static readonly Key[] _charKeys =
	[
		Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J,
		Key.K, Key.L, Key.M, Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T,
		Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z,
		Key.Number0, Key.Number1, Key.Number2, Key.Number3, Key.Number4,
		Key.Number5, Key.Number6, Key.Number7, Key.Number8, Key.Number9,
		Key.Space, Key.Minus, Key.Plus, Key.Period, Key.Comma, Key.Slash,
		Key.Semicolon, Key.BackSlash, Key.Quote, Key.BracketLeft, Key.BracketRight,
		Key.Tilde,
	];

	/// <summary>The root element of the GUI tree.</summary>
	public GuiElement? Root { get; set; }

	/// <summary>When false, input handling is skipped for this frame.</summary>
	public bool CanHandleEvent { get; set; } = true;

	/// <summary>Initializes the static input reference shared by all GUI elements.</summary>
	public static void InitInput(IInput input) => GuiElement.Input = input;

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
		var shift = input.IsKeyDown(Key.LShift) || input.IsKeyDown(Key.RShift);
		foreach (var key in _charKeys)
		{
			if (input.IsKeyPressed(key))
			{
				var c = KeyToChar(key, shift);
				if (c.HasValue) tb.HandleTextInput(c.Value);
			}
		}
	}

	/// <inheritdoc />
	public override void Draw(double alpha)
	{
		Root?.Draw(renderer);
	}

	private static char? KeyToChar(Key key, bool shift)
	{
		if (key is >= Key.A and <= Key.Z)
			return (char)(shift ? key - Key.A + 'A' : key - Key.A + 'a');
		if (key is >= Key.Number0 and <= Key.Number9)
			return shift ? ")!@#$%^&*("[(int)(key - Key.Number0)] : (char)(key - Key.Number0 + '0');
		return key switch
		{
			Key.Space => ' ',
			Key.Minus => shift ? '_' : '-',
			Key.Plus => shift ? '+' : '=',
			Key.Period => shift ? '>' : '.',
			Key.Comma => shift ? '<' : ',',
			Key.Slash => shift ? '?' : '/',
			Key.Semicolon => shift ? ':' : ';',
			Key.BackSlash => shift ? '|' : '\\',
			Key.Quote => shift ? '"' : '\'',
			Key.BracketLeft => shift ? '{' : '[',
			Key.BracketRight => shift ? '}' : ']',
			Key.Tilde => shift ? '~' : '`',
			_ => null
		};
	}
}