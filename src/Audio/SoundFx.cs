using System.IO;
using System.Numerics;
using CSCore;
using Silk.NET.OpenAL;
namespace LibAurora.Audio;

/// <summary>
/// A short sound effect that is fully loaded into memory.
/// Playback uses an OpenAL source obtained from the audio pool.
/// </summary>
public class SoundFx
{
	private readonly AL _al;
	private readonly uint _buffer;

	/// <summary>Creates a sound effect by decoding the entire audio stream into an OpenAL buffer.</summary>
	public unsafe SoundFx(AL al, IWaveSource source)
	{
		_al = al;
		_buffer = _al.GenBuffer();
		var memory = new MemoryStream();
		source.WriteToStream(memory);
		var data = memory.ToArray();
		if (data.Length == 0) throw new InvalidDataException("Audio data is empty");
		fixed (byte* ptr = data)
			_al.BufferData(_buffer,
				AudioUtils.GetFormat(source.WaveFormat),
				ptr,
				data.Length,
				source.WaveFormat.SampleRate);
	}

	/// <summary>Plays this sound effect on the given OpenAL source.</summary>
	public void Play(uint source, float volume, Vector2 position)
	{
		_al.SetSourceProperty(source, SourceInteger.Buffer, (int)_buffer);
		_al.SetSourceProperty(source, SourceFloat.Gain, volume);
		_al.SetSourceProperty(source, SourceVector3.Position, position.X, position.Y, 0.0f);
		_al.SourcePlay(source);
	}
}