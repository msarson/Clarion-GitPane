using System.Diagnostics;

namespace ICSharpCode.Core;

public class WinFormsDesigner
{
	public static bool IsInDesigner
	{
		get
		{
			Process currentProcess = Process.GetCurrentProcess();
			bool result = currentProcess.ProcessName == "devenv" || currentProcess.ProcessName == "SharpDevelop";
			currentProcess.Dispose();
			return result;
		}
	}
}
