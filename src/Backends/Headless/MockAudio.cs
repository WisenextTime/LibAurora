using System.Numerics;
using CSCore;
using LibAurora.Audio;
namespace LibAurora.Backends.Headless;

/// <summary>
/// Mock audio implementation. All methods are no-ops and creation methods return null.
/// Used in headless or testing scenarios.
/// </summary>
public class MockAudio : IAudio
{
	/// <inheritdoc />
	public Vector2 ListenerPosition { get; set; }

	/// <inheritdoc />
	public SoundFx? CreateSoundFx(IWaveSource stream) => null;

	/// <inheritdoc />
	public BgmPlayer? CreateBgmPlayer(params IWaveSource[] streams) => null;

	/// <inheritdoc />
	public void PlaySoundFx(SoundFx? sf, float volume = 1f, Vector2? position = null) { }
}