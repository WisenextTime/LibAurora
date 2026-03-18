using System.Numerics;
using CSCore;
namespace LibAurora.Audio;

public interface IAudio
{
	public Vector2 ListenerPosition { get; set; }

	public SoundFx? CreateSoundFx(IWaveSource stream);
	public BgmPlayer? CreateBgmPlayer(params IWaveSource[] streams);
	public void PlaySoundFx(SoundFx? sf, float volume = 1f, Vector2? position = null);
}
public enum AudioFormat
{
	Wav,
	Mp3,
	Ogg,
	Flac,
	Unknown,
}