using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Util;

public class ProcessRunner : IDisposable
{
	private enum ConsoleEvent
	{
		ControlC,
		ControlBreak
	}

	private Process process;

	private string standardOutput = string.Empty;

	private string workingDirectory = string.Empty;

	private OutputReader standardOutputReader;

	private OutputReader standardErrorReader;

	public string WorkingDirectory
	{
		get
		{
			return workingDirectory;
		}
		set
		{
			workingDirectory = value;
		}
	}

	public string StandardOutput
	{
		get
		{
			string result = string.Empty;
			if (standardOutputReader != null)
			{
				result = standardOutputReader.Output;
			}
			return result;
		}
	}

	public string StandardError
	{
		get
		{
			string result = string.Empty;
			if (standardErrorReader != null)
			{
				result = standardErrorReader.Output;
			}
			return result;
		}
	}

	public int ExitCode
	{
		get
		{
			int result = 0;
			if (process != null)
			{
				result = process.ExitCode;
			}
			return result;
		}
	}

	public bool IsRunning
	{
		get
		{
			bool result = false;
			if (process != null)
			{
				result = !process.HasExited;
			}
			return result;
		}
	}

	public event EventHandler ProcessExited;

	public event LineReceivedEventHandler OutputLineReceived;

	public event LineReceivedEventHandler ErrorLineReceived;

	public void Dispose()
	{
	}

	public void WaitForExit()
	{
		WaitForExit(int.MaxValue);
	}

	public bool WaitForExit(int timeout)
	{
		if (process == null)
		{
			throw new ProcessRunnerException(StringParser.Parse("${res:ICSharpCode.NAntAddIn.ProcessRunner.NoProcessRunningErrorText}"));
		}
		bool flag = process.WaitForExit(timeout);
		if (flag)
		{
			standardOutputReader.WaitForFinish();
			standardErrorReader.WaitForFinish();
		}
		return flag;
	}

	public void Start(string command, string arguments)
	{
		process = new Process();
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.FileName = command;
		process.StartInfo.WorkingDirectory = workingDirectory;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.Arguments = arguments;
		if (this.ProcessExited != null)
		{
			process.EnableRaisingEvents = true;
			process.Exited += OnProcessExited;
		}
		bool flag = false;
		try
		{
			process.Start();
			flag = true;
		}
		finally
		{
			if (!flag)
			{
				process.Exited -= OnProcessExited;
				process = null;
			}
		}
		standardOutputReader = new OutputReader(process.StandardOutput);
		if (this.OutputLineReceived != null)
		{
			standardOutputReader.LineReceived += OnOutputLineReceived;
		}
		standardOutputReader.Start();
		standardErrorReader = new OutputReader(process.StandardError);
		if (this.ErrorLineReceived != null)
		{
			standardErrorReader.LineReceived += OnErrorLineReceived;
		}
		standardErrorReader.Start();
	}

	public void Start(string command)
	{
		Start(command, string.Empty);
	}

	public void Kill()
	{
		if (process != null)
		{
			if (!process.HasExited)
			{
				process.Kill();
				process.Close();
				process.Dispose();
				process = null;
				standardOutputReader.WaitForFinish();
				standardErrorReader.WaitForFinish();
			}
			else
			{
				process = null;
			}
		}
	}

	protected void OnProcessExited(object sender, EventArgs e)
	{
		if (this.ProcessExited != null)
		{
			standardOutputReader.WaitForFinish();
			standardErrorReader.WaitForFinish();
			this.ProcessExited(this, e);
		}
	}

	protected void OnOutputLineReceived(object sender, LineReceivedEventArgs e)
	{
		if (this.OutputLineReceived != null)
		{
			this.OutputLineReceived(this, e);
		}
	}

	protected void OnErrorLineReceived(object sender, LineReceivedEventArgs e)
	{
		if (this.ErrorLineReceived != null)
		{
			this.ErrorLineReceived(this, e);
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);
}
