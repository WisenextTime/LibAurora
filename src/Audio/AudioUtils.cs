using System.IO;
using CSCore;
using CSCore.Codecs.OGG;
using CSCore.Codecs.WAV;
using LibAurora.Debug;
using Silk.NET.OpenAL;
namespace LibAurora.Audio;

/// <summary>
/// Static utilities for detecting, decoding, and querying audio formats.
/// </summary>
public static class AudioUtils
{
	/// <summary>
	/// Detects the audio format of a stream by reading its header.
	/// Supports WAV, Ogg Vorbis, MP3, and FLAC. Returns <see cref="AudioFormat.Unknown"/> on failure.
	/// The stream position is restored after detection.
	/// </summary>
	public static AudioFormat Detect(Stream audioStream)
	{
		StreamSeekException.ThrowIf(!audioStream.CanSeek);
		var originalPosition = audioStream.Position;
		try
		{
			var header = new byte[12];
			var bytesRead = audioStream.Read(header, 0, header.Length);
			if (bytesRead < 8)
				throw new InvalidDataException("Stream is too short to be a valid audio file.");
			if (header[0] == 0x4F && header[1] == 0x67 && header[2] == 0x67 && header[3] == 0x53)
				return AudioFormat.Ogg;
			if (header[0] == 0x66 && header[1] == 0x4C && header[2] == 0x61 && header[3] == 0x43)
				return AudioFormat.Flac;
			if (header[0] == 0x49 && header[1] == 0x44 && header[2] == 0x33)
				return AudioFormat.Mp3;
			if (header[0] == 0xFF && (header[1] & 0xFE) == 0xFA ||
			    header[0] == 0xFF && (header[1] & 0xFE) == 0xF2)
				return AudioFormat.Mp3;
			if (header[0] == 0x52 &&
			    header[1] == 0x49 &&
			    header[2] == 0x46 &&
			    header[3] == 0x46 &&
			    bytesRead >= 12 &&
			    header[8] == 0x57 &&
			    header[9] == 0x41 &&
			    header[10] == 0x56 &&
			    header[11] == 0x45)
				return AudioFormat.Wav;
			throw new InvalidDataException("Unknown audio file format.");
		}
		catch (IOException)
		{
			return AudioFormat.Unknown;
		}
		catch (InvalidDataException)
		{
			return AudioFormat.Unknown;
		}
		finally
		{
			audioStream.Position = originalPosition;
		}
	}

	/// <summary>Decodes an audio stream into an <see cref="IWaveSource"/>. Supports WAV and Ogg Vorbis.</summary>
	public static IWaveSource Decode(Stream audioStream)
	{
		var format = Detect(audioStream);
		return format switch
		{
			AudioFormat.Wav => new WaveFileReader(audioStream),
			AudioFormat.Ogg => new OggSource(audioStream).ToWaveSource(16),
			_ => throw new UnsupportedAudioFormat(format),
		};
	}

	/// <summary>Maps a CSCore <see cref="WaveFormat"/> to the corresponding OpenAL <see cref="BufferFormat"/>.</summary>
	public static BufferFormat GetFormat(WaveFormat format)
	{
		return (format.Channels, format.BitsPerSample) switch
		{
			(1, 16) => BufferFormat.Mono16,
			(1, 8) => BufferFormat.Mono8,
			(2, 16) => BufferFormat.Stereo16,
			(2, 8) => BufferFormat.Stereo8,
			_ => throw new UnsupportedAudioFormat(AudioFormat.Unknown)
		};
	}
}