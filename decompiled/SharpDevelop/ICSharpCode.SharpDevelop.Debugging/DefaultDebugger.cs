using System;
using System.Diagnostics;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Debugging;

public class DefaultDebugger : IDebugger, IDisposable
{
	private Process attachedProcess;

	public bool IsDebugging => attachedProcess != null;

	public bool IsProcessRunning => IsDebugging;

	public event EventHandler DebugStarted;

	public event EventHandler IsProcessRunningChanged;

	public event EventHandler DebugStopped;

	public bool CanDebug(IProject project)
	{
		return true;
	}

	public Process Start(ProcessStartInfo processStartInfo)
	{
		if (attachedProcess == null)
		{
			try
			{
				attachedProcess = new Process();
				attachedProcess.StartInfo = processStartInfo;
				attachedProcess.Exited += AttachedProcessExited;
				attachedProcess.EnableRaisingEvents = true;
				attachedProcess.Start();
				OnDebugStarted(EventArgs.Empty);
			}
			catch (Exception)
			{
				throw new ApplicationException("Can't execute \"" + processStartInfo.FileName + "\"\n");
			}
		}
		return attachedProcess;
	}

	private void AttachedProcessExited(object sender, EventArgs e)
	{
		attachedProcess.Exited -= AttachedProcessExited;
		attachedProcess.Dispose();
		attachedProcess = null;
		WorkbenchSingleton.SafeThreadAsyncCall(OnDebugStopped, EventArgs.Empty);
	}

	public void StartWithoutDebugging(ProcessStartInfo processStartInfo)
	{
		Process.Start(processStartInfo);
	}

	public void Attach(int ProcessId)
	{
	}

	public void Detach()
	{
	}

	public void Stop()
	{
		if (attachedProcess != null)
		{
			attachedProcess.Exited -= AttachedProcessExited;
			attachedProcess.Kill();
			attachedProcess.Close();
			attachedProcess.Dispose();
			attachedProcess = null;
		}
	}

	public void Break()
	{
		throw new NotSupportedException();
	}

	public void Continue()
	{
		throw new NotSupportedException();
	}

	public void StepInto()
	{
		throw new NotSupportedException();
	}

	public void StepOver()
	{
		throw new NotSupportedException();
	}

	public void StepOut()
	{
		throw new NotSupportedException();
	}

	public string GetValueAsString(string variable)
	{
		return null;
	}

	public DebuggerGridControl GetTooltipControl(string variable)
	{
		return null;
	}

	public bool CanSetInstructionPointer(string filename, int line, int column)
	{
		return false;
	}

	public bool SetInstructionPointer(string filename, int line, int column)
	{
		return false;
	}

	protected virtual void OnDebugStarted(EventArgs e)
	{
		if (this.DebugStarted != null)
		{
			this.DebugStarted(this, e);
		}
	}

	protected virtual void OnIsProcessRunningChanged(EventArgs e)
	{
		if (this.IsProcessRunningChanged != null)
		{
			this.IsProcessRunningChanged(this, e);
		}
	}

	protected virtual void OnDebugStopped(EventArgs e)
	{
		if (this.DebugStopped != null)
		{
			this.DebugStopped(this, e);
		}
	}

	public void Dispose()
	{
		Stop();
	}
}
