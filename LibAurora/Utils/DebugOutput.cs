using System;
using System.Diagnostics;
namespace LibAurora.Utils;

public static class DebugOutput
{
	[Conditional("DEBUG")]
	public static void Log(string msg, string type = "INFO")
	{
		Console.WriteLine($"[{type}] {msg}");
	}
}