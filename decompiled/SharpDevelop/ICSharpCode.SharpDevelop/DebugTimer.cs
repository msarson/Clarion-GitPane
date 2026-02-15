using System;
using System.Diagnostics;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public static class DebugTimer
{
	[ThreadStatic]
	private static Stopwatch stopWatch;

	[Conditional("DEBUG")]
	public static void Start()
	{
		if (stopWatch == null)
		{
			stopWatch = new Stopwatch();
		}
		stopWatch.Start();
	}

	[Conditional("DEBUG")]
	public static void Stop(string desc)
	{
		stopWatch.Stop();
		LoggingService.Debug("\"" + desc + "\" took " + stopWatch.ElapsedMilliseconds + " ms");
	}
}
