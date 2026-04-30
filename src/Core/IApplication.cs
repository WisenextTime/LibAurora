using System;
using System.Numerics;
using Veldrid.Sdl2;
namespace LibAurora.Core;

/// <summary>
/// Application lifecycle interface. Implementations manage window creation,
/// the main loop, and resource disposal.
/// </summary>
public interface IApplication : IDisposable
{
	/// <summary>Human-readable application name.</summary>
	public string Name { get; }

	/// <summary>True while the application's main loop is active.</summary>
	public bool IsRunning { get; }

	/// <summary>True after <see cref="IDisposable.Dispose"/> has been called.</summary>
	public bool IsDisposed { get; }

	/// <summary>Returns the type of this application (desktop, headless, etc.).</summary>
	ApplicationType Type();

	/// <summary>Starts the application and returns a context with graphics, input, and audio services.</summary>
	ApplicationContext Run(WindowCreateArguments args, SDL_WindowFlags? customFlags = null);

	/// <summary>Requests the application to stop running.</summary>
	void Exit();

	/// <summary>Pumps OS events and updates subsystems once per frame.</summary>
	public void Update();
}
/// <summary>Identifies the target platform / backend type.</summary>
public enum ApplicationType
{
	Desktop,
	Android,
	Headless,
}
/// <summary>Window creation parameters passed to <see cref="IApplication.Run"/>.</summary>
public struct WindowCreateArguments()
{
	/// <summary>Window title bar text. Default is "LibAurora Application".</summary>
	public string Title = "LibAurora Application";

	/// <summary>Window client area size in pixels.</summary>
	public Vector2 Size = new(1024, 720);

	/// <summary>Whether the window can be resized by the user.</summary>
	public bool Resizable = false;

	/// <summary>Whether to use high-DPI rendering.</summary>
	public bool HighDpi = false;
}