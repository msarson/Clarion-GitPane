using System;
using System.Threading;

namespace ICSharpCode.SharpDevelop.Debugging;

public class StartWaiter : MarshalByRefObject
{
	public static readonly AutoResetEvent AttachWaitEvent = new AutoResetEvent(initialState: false);

	private static int s_processID = 0;

	private static Version s_clrVersion;

	public static int ProcessID => s_processID;

	public static Version ClrVersion => s_clrVersion;

	public bool AttachToDebugger(int processID, Version clrVersion)
	{
		s_processID = processID;
		s_clrVersion = clrVersion;
		AttachWaitEvent.Set();
		return true;
	}
}
