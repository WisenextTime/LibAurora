using System.IO;
using System.Numerics;
using CSCore;
using LibAurora.Audio;
namespace LibAurora.Backends.Headless;

public class MockAudio : IAudio
{
	public Vector2 ListenerPosition { get; set; }
	public SoundFx? CreateSoundFx(IWaveSource stream) => null;
	public BgmPlayer? CreateBgmPlayer(params IWaveSource[] streams) => null;
	public void PlaySoundFx(SoundFx? sf, float volume = 1f, Vector2? position = null) { }
}