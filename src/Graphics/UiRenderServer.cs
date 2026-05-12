using System;
using LibAurora.Resources;
using Veldrid;
namespace LibAurora.Graphics;

/// <summary>Manages the shared UI renderer used by GUI and font systems.</summary>
public static class UiRenderServer
{
	private static UiRenderer? _renderer;

	/// <summary>The shared UI renderer instance.</summary>
	public static UiRenderer Renderer =>
		_renderer ?? throw new InvalidOperationException("UiRenderServer has not been initialized.");

	/// <summary>Initializes the shared UI renderer if needed and returns it.</summary>
	public static UiRenderer Init(IGraphics graphics)
	{
		if (_renderer is not null) return _renderer;

		var shaders = AssetServer.LoadResource<Shader[]>("ase://LibAurora/Shaders/Ui.glsl");
		_renderer = new UiRenderer(graphics, shaders);
		return _renderer;
	}

	/// <summary>Disposes the shared UI renderer.</summary>
	public static void Dispose()
	{
		_renderer?.Dispose();
		_renderer = null;
	}
}