using System;
using LibAurora.Core;
using Veldrid.Sdl2;
namespace LibAurora.Backends.Headless;

/// <summary>
/// Headless (no window) application implementation, useful for testing and server scenarios.
/// Provides mock input and audio services; graphics is not initialized.
/// </summary>
public class HeadlessApplication : IApplication
{
	/// <inheritdoc />
	public string Name { get; init; } = "Headless LibAurora Application";

	/// <inheritdoc />
	public bool IsRunning { get; private set; }

	/// <inheritdoc />
	public bool IsDisposed { get; private set; }

	/// <inheritdoc />
	public ApplicationType Type() => ApplicationType.Headless;

	/// <inheritdoc />
	public ApplicationContext Run(WindowCreateArguments args, SDL_WindowFlags? customFlags = null)
	{
		if (IsRunning) throw new InvalidOperationException("Already running");
		IsRunning = true;
		return new ApplicationContext
		{
			Application = this,
			Input = new MockInput(),
			Audio = new MockAudio(),
		};
	}

	/// <inheritdoc />
	public void Exit()
	{
		IsRunning = false;
	}

	/// <inheritdoc />
	public void Update() { }

	/// <inheritdoc />
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(IsDisposed, typeof(HeadlessApplication));
		IsDisposed = true;
		GC.SuppressFinalize(this);
	}
}