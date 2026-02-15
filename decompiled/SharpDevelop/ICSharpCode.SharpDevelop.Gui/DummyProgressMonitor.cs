namespace ICSharpCode.SharpDevelop.Gui;

public class DummyProgressMonitor : IProgressNotificationCenter
{
	bool IProgressNotificationCenter.ShowNotifications => false;

	void IProgressNotificationCenter.BeginTask(string taskName, int totalWork, bool allowCancel)
	{
	}

	void IProgressNotificationCenter.BeginTask(string taskName, string taskTet, int totalWork, bool allowCancel)
	{
	}

	void IProgressNotificationCenter.ShowNotification(string taskName, string taskText)
	{
	}

	void IProgressNotificationCenter.SetTaskTextAndWork(string taskName, string taskText, int workValue)
	{
	}

	int IProgressNotificationCenter.GetWorkDone(string taskName)
	{
		return 0;
	}

	void IProgressNotificationCenter.SetWorkDone(string taskName, int value)
	{
	}

	void IProgressNotificationCenter.SetTaskText(string taskName, string value)
	{
	}

	void IProgressNotificationCenter.IncreaseWorkDoneBy(string taskName, int value)
	{
	}

	void IProgressNotificationCenter.DecreaseWorkDoneBy(string taskName, int value)
	{
	}

	bool IProgressNotificationCenter.IsWorkDone(string taskName)
	{
		return false;
	}

	void IProgressNotificationCenter.Done(string taskName)
	{
	}

	bool IProgressNotificationCenter.GetIsCancelled(string taskName)
	{
		return true;
	}

	void IProgressNotificationCenter.SetIsCancelled(string taskName, bool value)
	{
	}

	bool IProgressNotificationCenter.TaskStarted(string taskName)
	{
		return false;
	}
}
