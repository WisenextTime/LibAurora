using System;
using System.Collections.Generic;
using CSCore;
using Silk.NET.OpenAL;
namespace LibAurora.Audio;

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

	private float _volume = 1.0f;
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
	public bool IsPlaying { get; private set; }

	public float Volume
	{
		get => _volume;
		set
		{
			_volume = Math.Clamp(value, 0f, 1f);
			_al.SetSourceProperty(_source, SourceFloat.Gain, _volume);
		}
	}

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
		IsPlaying = true;
	}
	public unsafe void Stop()
	{
		if (!IsPlaying) return;
		_al.SourceStop(_source);
		_al.GetSourceProperty(_source, GetSourceInteger.BuffersQueued, out var queued);
		if (queued > 0)
		{
			var processedBuffers = new uint[queued];
			fixed (uint* ptr = processedBuffers) _al.SourceUnqueueBuffers(_source, queued, ptr);
		}
		IsPlaying = false;
		_audios[_currentIndex].Position = 0;
	}
	public void Pause()
	{
		if (IsPlaying) _al.SourcePause(_source);
	}
	public void Resume()
	{
		if (IsPlaying) return;
		_al.SourcePlay(_source);
		IsPlaying = true;
	}
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
		if (IsPlaying && (SourceState)state != SourceState.Playing)
		{
			_al.SourcePlay(_source);
		}
	}
	public void AddAudio(IWaveSource stream)
		=> _audios.Add(stream);
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