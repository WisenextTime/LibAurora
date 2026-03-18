namespace LibAurora.Debug;

public static class LogServer
{
	public static ILogger? Logger;
	public static void Log(string message, LogLevel level = LogLevel.Info) => Logger?.Log(message, level);
}