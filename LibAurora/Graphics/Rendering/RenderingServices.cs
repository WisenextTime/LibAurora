using System;
using System.Collections.Generic;
using LibAurora.Framework;
using Raylib_cs;
namespace LibAurora.Graphics.Rendering;

public sealed class RenderingServices
{
	/// <summary>
	/// The size of the application window.
	/// </summary>
	public (int width, int height) WindowSize
	{
		get;
		set
		{
			field = value;
			if (Raylib.IsWindowReady()) Raylib.SetWindowSize(value.width, value.height);
		}
	} = (1280, 720);
	/// <summary>
	/// The title of the application window.
	/// </summary>
	public string Title
	{
		get;
		set
		{
			field = value;
			if (Raylib.IsWindowReady()) Raylib.SetWindowTitle(value);
		}
	} = "Lib Aurora Application";
	/// <summary>
	/// Get the RenderingService singleton
	/// </summary>
	/// <exception cref="InvalidOperationException">RenderingService is not initialized.</exception>
	public static RenderingServices Instance =>
		_instance ?? throw new InvalidOperationException("Rendering service not initialized");
	public void Register(params IRenderable[] renderables)
	{
		_renderables.AddRange(renderables);
	}

	private readonly List<IRenderable> _renderables = [];
	private bool _running;
	
	private static RenderingServices? _instance;
	internal RenderingServices()
	{
		if(_instance != null) throw new InvalidOperationException("Rendering service already been created");
		_instance = this;
	}

	internal void Initialize()
	{
		Raylib.InitWindow(WindowSize.width, WindowSize.height, Title);
	}

	internal void Run()
	{
		_running = true;
		while (_running)
		{
			Raylib.BeginDrawing();
			Draw();
			Raylib.EndDrawing();
		}
		Raylib.CloseWindow();
	}

	private void Draw()
	{
		foreach (var renderable in _renderables)
		{
			renderable.Draw();
		}
	}
}