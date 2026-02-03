namespace LibAurora.Timer;

public class Timer
{
	internal Timer() { }
	
	internal void Update(double delta)
	{
		if(Running && !Paused) _deltaTime += delta;
		if(_deltaTime > Time) OnTime?.Invoke();
		Running = false;
		if (Looped) Start();
	}
	public void Start()
	{
		if(Running) return;
		_deltaTime = 0;
		Running = true;
	}

	public void Reset() 
	{
		_deltaTime = 0;
	}

	public void Stop()
	{
		Running = false;
	}
	public bool Running { get; private set; } = false;
	public bool Looped { get; set; } = false;
	public bool Paused { get; set; } = false;
	public double Time { get; set; } = 1;
	public Action? OnTime;
	private double _deltaTime;
}