using System;
using System.Runtime.InteropServices;
using System.Text;
using LibAurora.Core;

namespace LibAurora.Backends.Desktop;

public static partial class Sdl2TextInput
{
	private const uint SdlTextInputEventType = 0x303;
	private readonly static SdlEventFilter _filter = OnEvent;
	private static bool _registered;

	[LibraryImport("SDL2")] private static partial void SDL_StartTextInput();
	[LibraryImport("SDL2")] private static partial void SDL_StopTextInput();
	[LibraryImport("SDL2")] private static partial void SDL_AddEventWatch(SdlEventFilter filter, IntPtr userdata);
	[LibraryImport("SDL2")] private static partial void SDL_DelEventWatch(SdlEventFilter filter, IntPtr userdata);
	[LibraryImport("SDL2")] private static partial void SDL_SetTextInputRect(ref SdlRect rect);

	public static void Start()
	{
		SDL_StartTextInput();
		if (_registered) return;
		SDL_AddEventWatch(_filter, IntPtr.Zero);
		_registered = true;
	}

	public static void Stop()
	{
		SDL_StopTextInput();
		if (!_registered) return;
		SDL_DelEventWatch(_filter, IntPtr.Zero);
		_registered = false;
	}

	public static void SetRect(uint x, uint y, uint w, uint h)
	{
		var rect = new SdlRect { x = (int)x, y = (int)y, w = (int)w, h = (int)h };
		SDL_SetTextInputRect(ref rect);
	}

	private static unsafe int OnEvent(IntPtr userdata, IntPtr sdlevent)
	{
		var ev = (RawTextInputEvent*)sdlevent;
		if (ev->type != SdlTextInputEventType) return 1;
		var len = 0;
		while (len < 32 && ev->text[len] != 0) len++;
		if (len == 0) return 1;
		var str = Encoding.UTF8.GetString(ev->text, len);
		Events.Raise(new Events.TextInputEvent(str));
		return 1;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct SdlRect
	{
		public int x;
		public int y;
		public int w;
		public int h;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int SdlEventFilter(IntPtr userdata, IntPtr sdlevent);

	[StructLayout(LayoutKind.Sequential)]
	private struct RawTextInputEvent
	{
		public uint type;
		public uint timestamp;
		public uint windowID;
		public unsafe fixed byte text[32];
	}
}