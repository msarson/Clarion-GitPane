using System;
using System.Diagnostics;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Debugging;

public interface IDebugger : IDisposable
{
	bool IsDebugging { get; }

	bool IsProcessRunning { get; }

	event EventHandler DebugStarted;

	event EventHandler IsProcessRunningChanged;

	event EventHandler DebugStopped;

	bool CanDebug(IProject project);

	Process Start(ProcessStartInfo processStartInfo);

	void StartWithoutDebugging(ProcessStartInfo processStartInfo);

	void Attach(int ProcessId);

	void Detach();

	void Stop();

	void Break();

	void Continue();

	void StepInto();

	void StepOver();

	void StepOut();

	string GetValueAsString(string variable);

	DebuggerGridControl GetTooltipControl(string variable);

	bool CanSetInstructionPointer(string filename, int line, int column);

	bool SetInstructionPointer(string filename, int line, int column);
}
