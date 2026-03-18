using System;
using LibAurora.Core;
using Veldrid.Sdl2;
namespace LibAurora.Backends.Headless;

public class HeadlessApplication : IApplication
{

	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(IsDisposed, typeof(HeadlessApplication));
		IsDisposed = true;
		GC.SuppressFinalize(this);
	}
	public string Name { get; set; } = "Headless LibAurora Application";
	public bool IsRunning { get; private set; }
	public bool IsDisposed { get; private set; }
	public ApplicationType Type() => ApplicationType.Headless;
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
	public void Exit()
	{
		IsRunning = false;
	}
	public void Update()
	{

	}
}