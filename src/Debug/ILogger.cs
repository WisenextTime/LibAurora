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
	/// <summary>Fatal error that may prevent normal operation.</summary>
	Error,
	/// <summary>Potentially harmful situation.</summary>
	Warning,
	/// <summary>General informational messages.</summary>
	Info,
	/// <summary>Detailed diagnostic output for troubleshooting.</summary>
	Debug,
}