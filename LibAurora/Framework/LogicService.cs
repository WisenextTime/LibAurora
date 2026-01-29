using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using LibAurora.Utils;
namespace LibAurora.Framework;

[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public sealed class LogicService
{
	/// <summary>
	/// The target ups(Update times per second) of logic loop.<br/>
	/// Only available before application run.
	/// </summary>
	public int TargetUps = 60;
	/// <summary>
	/// The max seconds running of one frame.<br/>
	/// Is should not usually be set to large.
	/// Only available before application run.
	/// </summary>
	public double MaxFrameTime = 0.25;
	/// <summary>
	/// The max update times per frame
	/// </summary>
	public int MaxUpf = 3;
	/// <summary>
	/// Time scale of logic.
	/// </summary>
	public float TimeScale = 1.0f;
	/// <summary>
	/// Get current update times per second)
	/// </summary>
	public int CurrentUps { get; private set; }
	/// <summary>
	/// Get the current interpolation factor (used for interpolation operation)
	/// </summary>
	public double InterpolationFactor { get; private set; }
	/// <summary>
	/// Get the LogicService singleton
	/// </summary>
	/// <exception cref="InvalidOperationException">LogicService is not initialized.</exception>
	public static LogicService Instance => _instance??throw new InvalidOperationException("Logic service not initialized");
	public void Register(params IUpdatable[] updatables)
	{
		_updatables.AddRange(updatables);
	}
	private readonly IMainLoop _mainLoop;
	private static LogicService? _instance;

	private readonly List<IUpdatable> _updatables = [];
	
	internal void Initialize()
	{
		_fixedTimeStep = 1.0f / TargetUps;
		_maxUpf = MaxUpf;
		_maxFrameTime = MaxFrameTime;
	}
	internal void Run()
	{
		_running = true;
		
		_gameTimer = Stopwatch.StartNew();
		_lastRealTime = _gameTimer.Elapsed.TotalSeconds;
		_logicThread.Start();
	}

	internal void Stop()
	{
		_running = false;
		_logicThread.Join(1000);
	}
	
	private Stopwatch _gameTimer = new();
	private readonly Thread _logicThread;
	private bool _running;
	
	private float _timeScale = 1.0f;
	private double _accumulatedGameTime;
	private double _lastRealTime;

	private double _fixedTimeStep;
	private int _maxUpf;
	private double _maxFrameTime;
	private int _updateCount;
	private double _upsTimer;
	internal LogicService(IMainLoop mainLoop)
	{
		if(_instance != null) throw new InvalidOperationException("Logic service already been created");
		_instance = this;
		_mainLoop = mainLoop;
		_logicThread = new Thread(FixedLogicLoop)
		{
			Name = "Logic Loop",
			Priority = ThreadPriority.AboveNormal,
		};
	}

	private void FixedLogicLoop()
	{
		while (_running)
		{
			var currentRealTime = _gameTimer.Elapsed.TotalSeconds;
			var realDeltaTime = currentRealTime - _lastRealTime;
			_lastRealTime = currentRealTime;
			
			_upsTimer += realDeltaTime;
			if (_upsTimer >= 1.0)
			{
				CurrentUps = _updateCount;
				_updateCount = 0;
				_upsTimer = 0;
				DebugOutput.Log($"UPS: {CurrentUps}");
			}
			
			if (realDeltaTime > _maxFrameTime)
			{
				realDeltaTime = _maxFrameTime;
				DebugOutput.Log($"Frame time too long : {realDeltaTime}. Limited to : {_maxFrameTime}", "WARNING");
			}
			
			if (TimeScale != _timeScale)
			{
				ApplyTimeScaleChange(TimeScale);
			}
			if (_timeScale == 0)
			{
				_accumulatedGameTime = 0;
				continue;
			}
            
			var gameDeltaTime = realDeltaTime * _timeScale;
			_accumulatedGameTime += gameDeltaTime;
            
			var updateCount = 0;
			while (_accumulatedGameTime >= _fixedTimeStep && updateCount < _maxUpf)
			{
				Update(_fixedTimeStep);
				_accumulatedGameTime -= _fixedTimeStep;
				updateCount++;
				_updateCount++;
			}
			InterpolationFactor = _accumulatedGameTime / _fixedTimeStep;
			Thread.Sleep(1);
		}
	}

	private void ApplyTimeScaleChange(float newScale)
	{
		if (newScale == _timeScale) return;

		if (_timeScale > 0 && newScale > 0)
			_accumulatedGameTime *= newScale / _timeScale;
		else if (newScale == 0 || _timeScale == 0 && newScale > 0)
			_accumulatedGameTime = 0;
		_timeScale = newScale;
	}

	private void Update(double deltaTime)
	{
		foreach (var updatable in _updatables)
		{
			updatable.Update(deltaTime);
		}
		_mainLoop.Update(deltaTime);
	}
}