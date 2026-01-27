using System;
using LibAurora.Core;
using LibAurora.Framework;
using LibAurora.Utils;
using Raylib_cs;
namespace LibAurora.Graphics.Rendering;

public class RenderingLoop
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
	/// The max rendering fps of the application
	/// </summary>
	public int TargetFps
	{
		get;
		set
		{
			field = value;
			Raylib.SetTargetFPS(value);
		}
	} = 120;
	
	private bool _running;
	private readonly IMainLoop _mainLoop;
	
	private static RenderingLoop? _instance;

	public static RenderingLoop Instance =>
		_instance ?? throw new InvalidOperationException("render service not exist");
	public RenderingLoop(IMainLoop mainLoop)
	{
		if(_instance != null) throw new InvalidOperationException("Render service already been created");
		_instance = this;
		_mainLoop = mainLoop;
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
		_mainLoop.Render();
	}
}