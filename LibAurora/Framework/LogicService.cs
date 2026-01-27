using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using LibAurora.Graphics.Rendering;
using LibAurora.Utils;
namespace LibAurora.Framework;

[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public class LogicLoop
{
	private readonly IMainLoop _mainLoop;
	/// <summary>
	/// The target ups of logic loop.<br/>
	/// Only available before application run.
	/// </summary>
	public int TargetUps = 60;
	public double MaxFrameTime = 0.25;
	public int MaxUpf = 3;
	public float TimeScale = 1.0f;
	public int CurrentUps { get; private set; }
	public double InterpolationFactor { get; private set; }
	
	public static LogicLoop Instance => _instance??throw new InvalidOperationException("Logic loop not exists");

	private static LogicLoop? _instance;
		
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
	public LogicLoop(IMainLoop mainLoop)
	{
		if(_instance != null) throw new InvalidOperationException("Logic loop already been created");
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
		_mainLoop.Update(deltaTime);
	}
}