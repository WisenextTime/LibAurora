using System;
using System.Linq;
using System.Runtime.CompilerServices;
using LibAurora.Debug;
using Veldrid;
using Veldrid.OpenGL;
using Veldrid.Sdl2;
namespace LibAurora.Utils;

public static class BackendCreateUtils
{
	public static GraphicsBackend? TestValidBackend(params GraphicsBackend[] backends)
		=> backends.First(GraphicsDevice.IsBackendSupported);

	public static unsafe SwapchainSource GetSwapchainSource(IntPtr windowHandle)
	{
		SDL_SysWMinfo sysWmInfo;
		Sdl2Native.SDL_GetVersion(&sysWmInfo.version);
		Sdl2Native.SDL_GetWMWindowInfo(windowHandle, &sysWmInfo);

		switch (sysWmInfo.subsystem)
		{
			case SysWMType.Windows: // Win32 supported.
				var w32Info = Unsafe.Read<Win32WindowInfo>(&sysWmInfo.info);
				return SwapchainSource.CreateWin32(w32Info.Sdl2Window, w32Info.hinstance);
			case SysWMType.X11: // Linux X11 supported.
				var x11Info = Unsafe.Read<X11WindowInfo>(&sysWmInfo.info);
				return SwapchainSource.CreateXlib(x11Info.display, x11Info.Sdl2Window);
			case SysWMType.Wayland: // Linux Wayland supported.
				var wlInfo = Unsafe.Read<WaylandWindowInfo>(&sysWmInfo.info);
				return SwapchainSource.CreateWayland(wlInfo.display, wlInfo.surface);
			case SysWMType.Cocoa: // MacOS supported. Despite this, I dont have a Mac to compile the Mac version EOP.
				var cocoaInfo = Unsafe.Read<CocoaWindowInfo>(&sysWmInfo.info);
				return SwapchainSource.CreateNSWindow(cocoaInfo.Window);
			case SysWMType.Android: // PART OF Android supported. Maybe I need more code to adapt to it in future.
				var androidInfo = Unsafe.Read<AndroidWindowInfo>(&sysWmInfo.info);
				return SwapchainSource.CreateAndroidSurface(androidInfo.surface, androidInfo.window);
			case SysWMType.UIKit: // I dont know why, but it seems that Veldrid does not support iOS.
			case SysWMType.DirectFB: // Linux DirectFB. Why would you play games on an embedded device?
			case SysWMType.Mir: // Ubuntu Mir, BUT WHY?
			case SysWMType.WinRT: // UWP Application.
			case SysWMType.Vivante: // I dont think you would enjoy playing games on an enbedded GPU device.
			case SysWMType.Unknown: // Your device is living off thr grid.
			default:
				throw new UnsupportedPlatformException(sysWmInfo.subsystem);
		}
	}

	public static GraphicsDevice CreateGraphicDevice(GraphicsBackend? backed,
		Sdl2Window window, SwapchainSource swapchainSource, GraphicsDeviceOptions option)
	{
		var swapchainDesc = new SwapchainDescription(
			source: swapchainSource,
			width: (uint)window.Width,
			height: (uint)window.Height,
			depthFormat: option.SwapchainDepthFormat,
			syncToVerticalBlank: option.SyncToVerticalBlank,
			colorSrgb: true);
		return backed switch
		{
			GraphicsBackend.Direct3D11 => GraphicsDevice.CreateD3D11(option, swapchainDesc),
			GraphicsBackend.Vulkan => GraphicsDevice.CreateVulkan(option, swapchainDesc),
			GraphicsBackend.OpenGL => GraphicsDevice.CreateOpenGL(option, CreateOpenGlPlatformInfo(window),
				(uint)window.Width,
				(uint)window.Height),
			GraphicsBackend.Metal => GraphicsDevice.CreateMetal(option, swapchainDesc),
			GraphicsBackend.OpenGLES => GraphicsDevice.CreateOpenGLES(option, swapchainDesc),
			_ => throw new UnknownRenderBackendException(),
		};
	}


	private static OpenGLPlatformInfo CreateOpenGlPlatformInfo(Sdl2Window window)
	{
		var context = Sdl2Native.SDL_GL_CreateContext(window.SdlWindowHandle);
		return new OpenGLPlatformInfo
		(
			openGLContextHandle: context,
			getProcAddress: Sdl2Native.SDL_GL_GetProcAddress,
			makeCurrent: obj => Sdl2Native.SDL_GL_MakeCurrent(window.SdlWindowHandle, obj),
			getCurrentContext: Sdl2Native.SDL_GL_GetCurrentContext,
			clearCurrentContext: () => Sdl2Native.SDL_GL_MakeCurrent(window.SdlWindowHandle, IntPtr.Zero),
			deleteContext: Sdl2Native.SDL_GL_DeleteContext,
			swapBuffers: () => Sdl2Native.SDL_GL_SwapWindow(window.SdlWindowHandle),
			setSyncToVerticalBlank: b => _ = Sdl2Native.SDL_GL_SetSwapInterval(b ? 1 : 0) == 0
		);
	}
}