using System;
using LibAurora.Audio;
using Veldrid;
using Veldrid.Sdl2;
namespace LibAurora.Debug;

public class UnknownRenderBackendException : Exception
{
	public override string Message => "Unknown rendering backend";
}
public class UnsupportedBackendException(GraphicsBackend backend) : Exception
{
	public override string Message => $"Unsupported platform: {backend}";
}
public class UnsupportedPlatformException(SysWMType platform) : Exception
{
	public override string Message => $"Unsupported platform: {platform}";
}
public class StreamSeekException : Exception
{
	public override string Message => "Stream can not be seeked.";
	public static void ThrowIf(bool condition)
	{
		if (condition) throw new StreamSeekException();
	}
}
public class UnsupportedAudioFormat(AudioFormat format) : Exception
{
	public override string Message => $"Unsupported audio format: {format}";
}