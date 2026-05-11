using System;
using LibAurora.Input;
using LibAurora.Resources;
using Myra;
using Veldrid;
namespace LibAurora.Graphics.Myra;

/// <summary>
/// Static entry point for the Myra retained-mode UI integration.
/// Initializes the Veldrid-backed renderer, platform, and root Desktop widget.
/// Call <see cref="Render"/> each frame between BeginFrame/EndFrame.
/// </summary>
public static class GuiServer
{
	private static MyraPlatform? _platform;
	public static MyraRenderer Renderer
		=> (MyraRenderer)(_platform?.Renderer ?? throw new InvalidOperationException("GuiServer has not been initialized."));

	/// <summary>Initializes the GUI server with the given input and graphics contexts. Loads shaders and creates the Myra platform.</summary>
	public static void Init(IInput input, IGraphics graphics)
	{
		var shaders = ResourceManager.LoadResource<Shader[]>("ase://LibAurora/Shaders/Myra.glsl");
		_platform = new MyraPlatform(graphics, input, shaders);
		MyraEnvironment.Platform = _platform;
	}
}