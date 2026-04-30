namespace LibAurora.Debug;

/// <summary>
/// Static log server. Set the <see cref="Logger"/> property to route log messages
/// to a custom implementation.
/// </summary>
public static class LogServer
{
	/// <summary>The current logger implementation. Null means logging is disabled.</summary>
	public static ILogger? Logger;

	/// <summary>Logs a message. Silently ignored if <see cref="Logger"/> is null.</summary>
	public static void Log(string message, LogLevel level = LogLevel.Info) => Logger?.Log(message, level);
}