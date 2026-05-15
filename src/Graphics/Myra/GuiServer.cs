using System;
using LibAurora.Input;
using Myra;
using Myra.Events;
using Myra.Graphics2D.UI;
namespace LibAurora.Graphics.Myra;

/// <summary>
/// Static entry point for the Myra retained-mode UI integration.
/// Initializes the Veldrid-backed renderer, platform, and root Desktop widget.
/// Call <see cref="Render"/> each frame between BeginFrame/EndFrame.
/// </summary>
public static class GuiServer
{
	private static IInput? _input;
	private static Desktop? _textInputDesktop;
	private static MyraPlatform? _platform;
	private static bool _textInputEnabled;
	public static UiRenderer Renderer => UiRenderServer.Renderer;

	/// <summary>Initializes the GUI server with the given input and graphics contexts. Loads shaders and creates the Myra platform.</summary>
	public static void Init(IInput input, IGraphics graphics)
	{
		_input = input;
		var renderer = UiRenderServer.Init(graphics);
		_platform = new MyraPlatform(graphics, input, renderer);
		MyraEnvironment.Platform = _platform;
	}

	public static void BindTextInput(Desktop desktop)
	{
		if (_textInputDesktop == desktop) return;
		UnbindTextInput();
		_textInputDesktop = desktop;
		desktop.WidgetGotKeyboardFocus += OnWidgetGotKeyboardFocus;
		desktop.WidgetLosingKeyboardFocus += OnWidgetLosingKeyboardFocus;
		UpdateTextInput(desktop.FocusedKeyboardWidget);
	}

	public static void UnbindTextInput()
	{
		if (_textInputDesktop == null) return;
		_textInputDesktop.WidgetGotKeyboardFocus -= OnWidgetGotKeyboardFocus;
		_textInputDesktop.WidgetLosingKeyboardFocus -= OnWidgetLosingKeyboardFocus;
		_textInputDesktop = null;
		SetTextInputEnabled(false);
	}

	public static void UpdateTextInputRect()
	{
		if (_textInputDesktop?.FocusedKeyboardWidget is TextBox textBox)
			SetTextInputRect(textBox);
	}

	private static void OnWidgetGotKeyboardFocus(object? sender, GenericEventArgs<Widget> e)
		=> UpdateTextInput(e.Data);

	private static void OnWidgetLosingKeyboardFocus(object? sender, CancellableEventArgs<Widget> e)
	{
		if (e.Data is TextBox) SetTextInputEnabled(false);
	}

	private static void UpdateTextInput(Widget? widget)
	{
		if (widget is not TextBox textBox)
		{
			SetTextInputEnabled(false);
			return;
		}

		SetTextInputRect(textBox);
		SetTextInputEnabled(true);
	}

	private static void SetTextInputRect(TextBox textBox)
	{
		var bounds = textBox.ActualBounds;
		_input?.SetTextInputRect(
			(uint)Math.Max(0, bounds.X),
			(uint)Math.Max(0, bounds.Y),
			(uint)Math.Max(1, bounds.Width),
			(uint)Math.Max(1, bounds.Height));
	}

	private static void SetTextInputEnabled(bool enabled)
	{
		if (_textInputEnabled == enabled) return;
		_textInputEnabled = enabled;
		if (enabled) _input?.StartTextInput();
		else _input?.EndTextInput();
	}
}