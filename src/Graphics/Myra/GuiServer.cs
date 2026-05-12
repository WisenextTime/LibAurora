using LibAurora.Input;
using Myra;
namespace LibAurora.Graphics.Myra;

/// <summary>
/// Static entry point for the Myra retained-mode UI integration.
/// Initializes the Veldrid-backed renderer, platform, and root Desktop widget.
/// Call <see cref="Render"/> each frame between BeginFrame/EndFrame.
/// </summary>
public static class GuiServer
{
	private static MyraPlatform? _platform;
	public static UiRenderer Renderer => UiRenderServer.Renderer;

	/// <summary>Initializes the GUI server with the given input and graphics contexts. Loads shaders and creates the Myra platform.</summary>
	public static void Init(IInput input, IGraphics graphics)
	{
		var renderer = UiRenderServer.Init(graphics);
		_platform = new MyraPlatform(graphics, input, renderer);
		MyraEnvironment.Platform = _platform;
	}
}