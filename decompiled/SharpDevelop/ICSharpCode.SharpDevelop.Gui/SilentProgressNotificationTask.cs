namespace ICSharpCode.SharpDevelop.Gui;

public class SilentProgressNotificationTask : IProgressNotificationTask
{
	private int totalWork;

	private int workDone;

	private string taskName;

	private string taskText;

	private bool allowCancel;

	private bool _IsCancelled;

	public string TaskText
	{
		get
		{
			return taskText;
		}
		set
		{
			taskText = value;
		}
	}

	public int WorkDone
	{
		get
		{
			return workDone;
		}
		set
		{
			workDone = value;
		}
	}

	public bool IsWorkDone
	{
		get
		{
			if (workDone > 0)
			{
				return workDone >= totalWork;
			}
			return false;
		}
	}

	public string TaskName
	{
		get
		{
			return taskName;
		}
		set
		{
			taskName = value;
		}
	}

	public bool IsCancelled
	{
		get
		{
			return _IsCancelled;
		}
		set
		{
			_IsCancelled = value;
		}
	}

	public void BeginTask(string name, int totalWork, bool allowCancel)
	{
		taskName = name;
		this.totalWork = totalWork;
		this.allowCancel = allowCancel;
	}

	public void BeginTask(string taskName, string initialTaskText, int totalWork, bool allowCancel)
	{
		this.taskName = taskName;
		taskText = initialTaskText;
		this.totalWork = totalWork;
		this.allowCancel = allowCancel;
	}

	public void Done()
	{
		workDone = 0;
		taskName = string.Empty;
		totalWork = 0;
	}

	public void Show()
	{
	}
}
