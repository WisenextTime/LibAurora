using System.Numerics;
using CSCore;
namespace LibAurora.Audio;

/// <summary>
/// Audio subsystem interface. Provides sound effect creation, BGM playback,
/// and listener position management.
/// </summary>
public interface IAudio
{
	/// <summary>The listener's position in 2D space, used for spatial audio.</summary>
	public Vector2 ListenerPosition { get; set; }

	/// <summary>Creates a sound effect from an audio stream. Returns null if not supported.</summary>
	public SoundFx? CreateSoundFx(IWaveSource stream);

	/// <summary>Creates a streaming BGM player from one or more audio streams. Returns null if not supported.</summary>
	public BgmPlayer? CreateBgmPlayer(params IWaveSource[] streams);

	/// <summary>Plays a sound effect with optional volume and spatial position.</summary>
	public void PlaySoundFx(SoundFx? sf, float volume = 1f, Vector2? position = null);
}
/// <summary>Supported audio file formats.</summary>
public enum AudioFormat
{
	Wav,
	Mp3,
	Ogg,
	Flac,
	Unknown,
}