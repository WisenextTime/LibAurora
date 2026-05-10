using System;
using LibAurora.Input;
using LibAurora.Resources;
using Myra;
using Myra.Graphics2D.UI;
using Veldrid;
namespace LibAurora.Graphics.Myra;

/// <summary>
/// Static entry point for the Myra retained-mode UI integration.
/// Initializes the Veldrid-backed renderer, platform, and root Desktop widget.
/// Call <see cref="Render"/> each frame between BeginFrame/EndFrame.
/// </summary>
public static class GuiServer
{
	private static Desktop? _root;

	/// <summary>The root Myra <see cref="Desktop"/> widget. Throws if not initialized via <see cref="Init"/>.</summary>
	public static Desktop Desktop => _root ?? throw new InvalidOperationException("GuiServer has not been initialized.");

	/// <summary>Initializes the GUI server with the given input and graphics contexts. Loads shaders and creates the Myra platform.</summary>
	public static void Init(IInput input, IGraphics graphics)
	{
		var shaders = ResourceManager.LoadResource<Shader[]>("ase://LibAurora/Shaders/Myra.glsl");
		MyraEnvironment.Platform = new MyraPlatform(graphics, input, shaders);
		_root = new Desktop();
	}

	/// <summary>Renders one frame of the Myra UI.</summary>
	public static void Render() => Desktop.Render();
}