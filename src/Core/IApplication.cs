using System;
using System.Numerics;
using Veldrid.Sdl2;
namespace LibAurora.Core;

public interface IApplication : IDisposable
{
	public string Name { get; }
	public bool IsRunning { get; }
	public bool IsDisposed { get; }
	ApplicationType Type();
	ApplicationContext Run(WindowCreateArguments args, SDL_WindowFlags? customFlags = null);
	void Exit();
	public void Update();
}
public enum ApplicationType
{
	Desktop,
	Android,
	Headless,
}
public struct WindowCreateArguments()
{
	public string Title = "LibAurora Application";
	public Vector2 Size = new(1024, 720);

	public bool Resizable = false;
	public bool HighDpi = false;
}