using System;
using System.Numerics;
using LibAurora.Core;
using Veldrid;
using Veldrid.Sdl2;
namespace LibAurora.Backends.Desktop;

/// <summary>
/// Desktop application implementation using SDL2 windowing.
/// Creates window, graphics, input, and audio subsystems on <see cref="Run"/>.
/// </summary>
public class DesktopApplication : IApplication
{
	private DesktopAudio? _audio;
	private DesktopInput? _input;
	private Vector2 _lastWindowSize = Vector2.Zero;
	private Sdl2Window? _window;
	private Vector2 WindowSize => new(_window?.Width ?? 0, _window?.Height ?? 0);

	/// <inheritdoc />
	public string Name { get; init; } = "LibAuroraApplication";

	/// <inheritdoc />
	public bool IsRunning { get; private set; }

	/// <inheritdoc />
	public bool IsDisposed { get; private set; }

	/// <inheritdoc />
	public ApplicationType Type() => ApplicationType.Desktop;

	/// <inheritdoc />
	public ApplicationContext Run(WindowCreateArguments args, SDL_WindowFlags? customFlags = null)
	{
		if (IsRunning) throw new InvalidOperationException("Already running");
		IsRunning = true;
		var flags = ParseFlags(args, customFlags);
		var graphics = new DesktopGraphics();
		if (graphics.Backend is GraphicsBackend.OpenGL or GraphicsBackend.OpenGLES)
			flags |= SDL_WindowFlags.OpenGL;
		_window = new Sdl2Window(args.Title, 100, 100, (int)args.Size.X, (int)args.Size.Y, flags, false);
		graphics.Initialize(_window);
		_input = new DesktopInput(_window);
		DesktopInput.EnableTextInput();
		OnResize();
		_window.Resized += OnResize;
		_window.SetCloseRequestedHandler(OnClosing);
		_audio = new DesktopAudio();
		_audio.Init();
		return new ApplicationContext
		{
			Application = this,
			Graphics = graphics,
			Input = _input,
			Audio = _audio,
		};
	}

	/// <inheritdoc />
	public void Exit()
	{
		ObjectDisposedException.ThrowIf(IsDisposed, typeof(DesktopApplication));
		IsRunning = false;
		_window?.Close();
	}

	/// <inheritdoc />
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(IsDisposed, typeof(DesktopApplication));
		IsDisposed = true;
		Exit();
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public void Update()
	{
		_window?.PumpEvents();
		_audio?.Update();
	}

	private static SDL_WindowFlags ParseFlags(WindowCreateArguments args, SDL_WindowFlags? customFlags)
	{
		var flags = SDL_WindowFlags.Shown;
		if (args.Resizable) flags |= SDL_WindowFlags.Resizable;
		if (args.HighDpi) flags |= SDL_WindowFlags.AllowHighDpi;
		if (customFlags.HasValue) flags |= customFlags.Value;
		return flags;
	}

	private void OnResize()
	{
		if (WindowSize == _lastWindowSize) return;
		_lastWindowSize = WindowSize;
		Events.Raise(new Events.SurfaceResizeEvent(WindowSize));
	}

	private bool OnClosing()
	{
		Events.Raise(new Events.SurfaceCloseEvent());
		return true;
	}
}