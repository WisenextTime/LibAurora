using LibAurora.Framework;
namespace LibAurora.Timer;

public class TimerService :IUpdatable
{
	/// <summary>
	/// Get the TimerService singleton
	/// </summary>
	/// <exception cref="InvalidOperationException">TimerService is not initialized.</exception>
	public static TimerService Instance => _instance ?? throw new InvalidOperationException("Input service is not initialized.");
	public TimerService()
	{
		if(_instance != null) throw new InvalidOperationException("Input service already been created");
		_instance = this;
	}
	private static TimerService? _instance;

	private readonly List<Timer> _timers = [];

	public Timer CreateTimer()
	{
		var timer = new Timer();
		_timers.Add(timer);
		return timer;
	}
	
	public void RemoveTimer(Timer timer) => _timers.Remove(timer);
	public void Update(double delta)
	{
		foreach(var timer in _timers) timer.Update(delta);
	}
}