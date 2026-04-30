namespace LibAurora.Debug;

/// <summary>Interface for log output implementations.</summary>
public interface ILogger
{
	/// <summary>Logs a message with the specified severity level.</summary>
	void Log(string message, LogLevel level);
}
/// <summary>Log severity levels.</summary>
public enum LogLevel
{
	Error,
	Warning,
	Info,
	Debug,
}