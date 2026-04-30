using System.Collections.Generic;
using System.Numerics;
using CSCore;
using LibAurora.Audio;
using LibAurora.Debug;
using Silk.NET.OpenAL;
namespace LibAurora.Backends.Desktop;

/// <summary>
/// Desktop audio implementation using OpenAL via Silk.NET.
/// Manages listener position, sound effect source pool, and BGM player collection.
/// </summary>
public class DesktopAudio(uint sfxMaxCount = 128) : IAudio
{
	private readonly AL _al = AL.GetApi();
	private readonly ALContext _alContext = ALContext.GetApi();
	private readonly List<BgmPlayer> _players = [];
	private readonly Queue<uint> _sourcePool = [];
	private List<uint> _activeSource = [];
	private unsafe Context* _context;

	private unsafe Device* _device;

	/// <inheritdoc />
	public Vector2 ListenerPosition
	{
		get;
		set
		{
			if (value == field) return;
			SetListener(value);
			field = value;
		}
	} = Vector2.Zero;

	/// <inheritdoc />
	public SoundFx CreateSoundFx(IWaveSource stream)
	{
		return new SoundFx(_al, stream);
	}

	/// <inheritdoc />
	public BgmPlayer CreateBgmPlayer(params IWaveSource[] streams)
	{
		var player = new BgmPlayer(_al, streams);
		_players.Add(player);
		return player;
	}

	/// <inheritdoc />
	public void PlaySoundFx(SoundFx? sf, float volume = 1f, Vector2? position = null)
	{
		CheckActiveSource();
		if (!_sourcePool.TryDequeue(out var source)) return;
		_activeSource.Add(source);
		sf?.Play(source, volume, position ?? Vector2.Zero);
	}

	/// <summary>Initializes the OpenAL device, context, and sound effect source pool.</summary>
	public unsafe void Init()
	{
		_device = _alContext.OpenDevice(null);
		if (_device == null) throw new AudioDeviceException("Could not open audio device");
		_context = _alContext.CreateContext(_device, null);
		if (_context == null) throw new AudioContextException("Could not create audio context");
		_alContext.MakeContextCurrent(_context);
		SetListener(ListenerPosition);
		InitListener();
		for (var i = 0; i < sfxMaxCount; i++)
		{
			var source = _al.GenSource();
			_sourcePool.Enqueue(source);
		}
		LogServer.Log("Audio system initialized", LogLevel.Debug);
	}

	/// <summary>Updates all active BGM players. Call once per frame.</summary>
	public void Update()
	{
		foreach (var player in _players) player.Update();
	}

	private void CheckActiveSource()
	{
		for (var i = _activeSource.Count - 1; i >= 0; i--)
		{
			var source = _activeSource[i];
			_al.GetSourceProperty(source, GetSourceInteger.SourceState, out var result);
			if ((SourceState)result != SourceState.Stopped) continue;
			_activeSource.RemoveAt(i);
			_sourcePool.Enqueue(source);
			_al.SetSourceProperty(source, SourceInteger.Buffer, 0);
		}
	}

	private void SetListener(Vector2 position)
	{
		_al.SetListenerProperty(ListenerVector3.Position, position.X, position.Y, 0.0f);
	}

	private unsafe void InitListener()
	{
		_al.SetListenerProperty(ListenerFloat.Gain, 1.0f);
		var ptr = stackalloc float[6];
		ptr[0] = 0.0f;
		ptr[1] = 0.0f;
		ptr[2] = -1.0f;
		ptr[3] = 0.0f;
		ptr[4] = 1.0f;
		ptr[5] = 0.0f;
		_al.SetListenerProperty(ListenerFloatArray.Orientation, ptr);
	}
}