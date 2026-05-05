using System;
using LibAurora.Core;
using LibAurora.Resources;
using Veldrid.Sdl2;
namespace LibAurora.Backends.Headless;

/// <summary>
/// Headless (no window) application implementation, useful for testing and server scenarios.
/// Provides mock input and audio services; graphics is not initialized.
/// </summary>
public class HeadlessApplication : IApplication
{
	/// <inheritdoc />
	public string Name { get; init; } = "LibAuroraApplication";

	/// <inheritdoc />
	public bool IsRunning { get; private set; }

	/// <inheritdoc />
	public bool IsDisposed { get; private set; }

	/// <inheritdoc />
	public ApplicationType Type() => ApplicationType.Headless;

	/// <inheritdoc />
	public ApplicationContext Run(WindowCreateArguments args, SDL_WindowFlags? customFlags = null)
	{
		ObjectDisposedException.ThrowIf(IsDisposed, typeof(HeadlessApplication));
		if (IsRunning) throw new InvalidOperationException("Application already running");
		IsRunning = true;
		return new ApplicationContext
		{
			Application = this,
			Input = new MockInput(),
			Audio = new MockAudio(),
			Resources = new DesktopResources(Name),
		};
	}

	/// <inheritdoc />
	public void Exit()
	{
		ObjectDisposedException.ThrowIf(IsDisposed, typeof(HeadlessApplication));
		IsRunning = false;
	}

	/// <inheritdoc />
	public void Update() { }

	/// <inheritdoc />
	public event Func<bool>? OnCloseEventHandler;

	/// <inheritdoc />
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(IsDisposed, typeof(HeadlessApplication));
		IsDisposed = true;
		Exit();
		GC.SuppressFinalize(this);
	}
}