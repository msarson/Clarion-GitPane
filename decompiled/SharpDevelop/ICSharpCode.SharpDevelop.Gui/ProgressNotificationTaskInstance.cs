using System;

namespace ICSharpCode.SharpDevelop.Gui;

public class ProgressNotificationTaskInstance : IProgressNotificationTaskInstance, IDisposable
{
	private Guid _taskId;

	private string _taskName;

	public int WorkDone
	{
		get
		{
			return StatusBarService.ProgressMonitor.GetWorkDone(_taskName);
		}
		set
		{
			StatusBarService.ProgressMonitor.SetWorkDone(_taskName, value);
		}
	}

	public string TaskText
	{
		set
		{
			StatusBarService.ProgressMonitor.SetTaskText(_taskName, value);
		}
	}

	public bool IsWorkDone => StatusBarService.ProgressMonitor.IsWorkDone(_taskName);

	public bool TaskStarted => StatusBarService.ProgressMonitor.TaskStarted(_taskName);

	public bool IsCancelled
	{
		get
		{
			return StatusBarService.ProgressMonitor.GetIsCancelled(_taskName);
		}
		set
		{
			StatusBarService.ProgressMonitor.SetIsCancelled(_taskName, value);
		}
	}

	public ProgressNotificationTaskInstance(string initialTaskText)
		: this(initialTaskText, allowCancel: false)
	{
	}

	public ProgressNotificationTaskInstance(string initialTaskText, bool allowCancel)
		: this(initialTaskText, 0, allowCancel)
	{
	}

	public ProgressNotificationTaskInstance(string initialTaskText, int totalWork, bool allowCancel)
	{
		_taskId = Guid.NewGuid();
		_taskName = _taskId.ToString();
		BeginTask(initialTaskText, totalWork, allowCancel);
	}

	public void BeginTask(string initialTaskText, int totalWork, bool allowCancel)
	{
		if (string.IsNullOrEmpty(initialTaskText))
		{
			initialTaskText = "${res:Global.PleaseWait}";
		}
		if (!StatusBarService.ProgressMonitor.TaskStarted(_taskName) || !StatusBarService.ProgressMonitor.IsWorkDone(_taskName))
		{
			StatusBarService.ProgressMonitor.BeginTask(_taskName, initialTaskText, totalWork, allowCancel);
		}
	}

	public void IncreseWorkDoneBy(int value)
	{
		StatusBarService.ProgressMonitor.IncreaseWorkDoneBy(_taskName, value);
	}

	public void DecreseWorkDoneBy(int value)
	{
		StatusBarService.ProgressMonitor.DecreaseWorkDoneBy(_taskName, value);
	}

	public void Done()
	{
		StatusBarService.ProgressMonitor.Done(_taskName);
	}

	public void Dispose()
	{
		if (_taskName != null)
		{
			try
			{
				Done();
				_taskName = null;
			}
			catch
			{
			}
		}
	}
}
