using System;
using System.Collections.Generic;
using CSCore;
using Silk.NET.OpenAL;
namespace LibAurora.Audio;

/// <summary>
/// Streamed background music player using OpenAL buffer queuing.
/// Supports multiple tracks, play/pause/resume/stop, volume control, and track management.
/// </summary>
public class BgmPlayer : IDisposable
{
	private const int BufferCount = 4;
	private const int BufferSize = 64 * 1024;
	private readonly AL _al;
	private readonly List<IWaveSource> _audios;
	private readonly uint[] _buffers;
	private readonly byte[] _pcmBuffer = new byte[BufferSize];
	private readonly uint _source;
	private int _currentIndex;
	private bool _isPaused;
	private float _volume = 1.0f;

	/// <summary>Creates a new BGM player from one or more audio streams.</summary>
	/// <exception cref="ArgumentException">Thrown when no audio streams are provided.</exception>
	public BgmPlayer(AL al, params IWaveSource[] audioStreams)
	{
		_al = al;
		_audios = [.. audioStreams];
		_currentIndex = 0;
		if (_audios.Count == 0) throw new ArgumentException("No audio.");
		_source = _al.GenSource();
		_al.SetSourceProperty(_source, SourceBoolean.SourceRelative, true);
		_al.SetSourceProperty(_source, SourceVector3.Position, 0f, 0f, 0f);
		_al.SetSourceProperty(_source, SourceFloat.ReferenceDistance, 0f);
		_al.SetSourceProperty(_source, SourceFloat.RolloffFactor, 0f);
		_al.SetSourceProperty(_source, SourceFloat.Gain, _volume);
		_buffers = new uint[BufferCount];
		for (var i = 0; i < BufferCount; i++) _buffers[i] = _al.GenBuffer();
	}

	/// <summary>True while audio is actively playing (not paused or stopped).</summary>
	public bool IsPlaying { get; private set; }

	/// <summary>True while playback is paused. Use <see cref="Resume"/> to continue.</summary>
	public bool IsPaused => _isPaused;

	/// <summary>Volume level between 0.0 and 1.0.</summary>
	public float Volume
	{
		get => _volume;
		set
		{
			_volume = Math.Clamp(value, 0f, 1f);
			_al.SetSourceProperty(_source, SourceFloat.Gain, _volume);
		}
	}

	/// <summary>Releases all OpenAL buffers and the source.</summary>
	public void Dispose()
	{
		Stop();
		foreach (var buffer in _buffers)
		{
			if (buffer != 0) _al.DeleteBuffer(buffer);
		}
		if (_source != 0) _al.DeleteSource(_source);
		GC.SuppressFinalize(this);
	}

	/// <summary>Starts or restarts playback from the beginning of the current track.</summary>
	public unsafe void Play()
	{
		if (IsPlaying) return;
		_al.GetSourceProperty(_source, GetSourceInteger.BuffersQueued, out var queued);
		if (queued > 0)
		{
			var temp = new uint[queued];
			fixed (uint* ptr = temp) _al.SourceUnqueueBuffers(_source, queued, ptr);
		}
		_audios[_currentIndex].Position = 0;
		var filledBuffers = 0;
		for (var i = 0; i < BufferCount; i++)
		{
			if (FillBuffer(_buffers[i], _audios[_currentIndex]))
				filledBuffers++;
			else
				break;
		}
		fixed (uint* ptr = _buffers) _al.SourceQueueBuffers(_source, filledBuffers, ptr);
		_al.SourcePlay(_source);
		_isPaused = false;
		IsPlaying = true;
	}

	/// <summary>Stops playback and resets the track position.</summary>
	public unsafe void Stop()
	{
		if (!IsPlaying) return;
		_al.SourceStop(_source);
		_isPaused = false;
		_al.GetSourceProperty(_source, GetSourceInteger.BuffersQueued, out var queued);
		if (queued > 0)
		{
			var processedBuffers = new uint[queued];
			fixed (uint* ptr = processedBuffers) _al.SourceUnqueueBuffers(_source, queued, ptr);
		}
		IsPlaying = false;
		_audios[_currentIndex].Position = 0;
	}

	/// <summary>Pauses playback. <see cref="IsPlaying"/> remains true; use <see cref="Resume"/> to continue.</summary>
	public void Pause()
	{
		if (!IsPlaying || _isPaused) return;
		_al.SourcePause(_source);
		_isPaused = true;
	}

	/// <summary>Resumes playback after a pause. Has no effect if not paused or if stopped.</summary>
	public void Resume()
	{
		if (!_isPaused || IsPlaying) return;
		_al.SourcePlay(_source);
		_isPaused = false;
	}

	/// <summary>Called each frame to refill processed buffers and advance to the next track if needed.</summary>
	public unsafe void Update()
	{
		if (!IsPlaying) return;
		_al.GetSourceProperty(_source, GetSourceInteger.BuffersProcessed, out var processed);
		if (processed > 0)
		{
			var processedBuffers = new uint[processed];
			fixed (uint* ptr = processedBuffers) _al.SourceUnqueueBuffers(_source, processed, ptr);
			for (var i = 0; i < processed; i++)
			{
				var buffer = processedBuffers[i];
				if (FillBuffer(buffer, _audios[_currentIndex]))
					_al.SourceQueueBuffers(_source, 1, &buffer);
				else
				{
					NextTrack();
					if (FillBuffer(buffer, _audios[_currentIndex])) _al.SourceQueueBuffers(_source, 1, &buffer);
				}
			}
		}
		_al.GetSourceProperty(_source, GetSourceInteger.SourceState, out var state);
		if (IsPlaying && !_isPaused && (SourceState)state != SourceState.Playing)
		{
			_al.SourcePlay(_source);
		}
	}

	/// <summary>Adds a new track to the end of the playlist.</summary>
	public void AddAudio(IWaveSource stream)
		=> _audios.Add(stream);

	/// <summary>Removes a track from the playlist. Adjusts current index if necessary.</summary>
	public void RemoveAudio(IWaveSource stream)
	{
		var index = _audios.IndexOf(stream);
		if (index == -1) return;
		_audios.RemoveAt(index);
		if (_audios.Count == 0)
		{
			_currentIndex = -1;
			Stop();
		}
		else if (index <= _currentIndex)
		{
			_currentIndex = (_currentIndex - 1 + _audios.Count) % _audios.Count;
		}
	}

	private unsafe bool FillBuffer(uint buffer, IWaveSource stream)
	{
		var bytesRead = stream.Read(_pcmBuffer, 0, BufferSize);
		if (bytesRead == 0) return false;
		fixed (byte* ptr = _pcmBuffer)
		{
			_al.BufferData(buffer, AudioUtils.GetFormat(stream.WaveFormat), ptr, bytesRead,
				stream.WaveFormat.SampleRate);
		}
		return true;
	}

	private void NextTrack()
	{
		_currentIndex = (_currentIndex + 1) % _audios.Count;
		_audios[_currentIndex].Position = 0;
	}
}