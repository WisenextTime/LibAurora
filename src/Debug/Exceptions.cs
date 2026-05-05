using System;
using LibAurora.Audio;
using Veldrid;
using Veldrid.Sdl2;
namespace LibAurora.Debug;

/// <summary>Thrown when the render backend type is not recognized.</summary>
public class UnknownRenderBackendException : Exception
{
	public override string Message => "Unknown rendering backend";
}
/// <summary>Thrown when the specified <see cref="GraphicsBackend"/> is not supported on the current platform.</summary>
public class UnsupportedBackendException(GraphicsBackend backend) : Exception
{
	public override string Message => $"Unsupported backend: {backend}";
}
/// <summary>Thrown when the current <paramref name="platform"/> type is unsupported.</summary>
public class UnsupportedPlatformException(SysWMType platform) : Exception
{
	public override string Message => $"Unsupported platform: {platform}";
}
/// <summary>Thrown when a stream does not support seeking.</summary>
public class StreamSeekException : Exception
{
	public override string Message => "Stream can not be seeked.";
	/// <summary>Throws a <see cref="StreamSeekException"/> if the condition is true.</summary>
	public static void ThrowIf(bool condition)
	{
		if (condition) throw new StreamSeekException();
	}
}
/// <summary>Thrown when an <see cref="AudioFormat"/> is not supported by the audio backend.</summary>
public class UnsupportedAudioFormat(AudioFormat format) : Exception
{
	public override string Message => $"Unsupported audio format: {format}";
}