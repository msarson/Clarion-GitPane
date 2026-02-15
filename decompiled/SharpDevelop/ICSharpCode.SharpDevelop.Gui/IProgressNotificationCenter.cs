namespace ICSharpCode.SharpDevelop.Gui;

public interface IProgressNotificationCenter
{
	bool ShowNotifications { get; }

	void BeginTask(string taskName, int totalWork, bool allowCancel);

	void BeginTask(string taskName, string initialTaskText, int totalWork, bool allowCancel);

	void ShowNotification(string taskName, string taskText);

	int GetWorkDone(string taskName);

	void SetWorkDone(string taskName, int value);

	void SetTaskText(string taskName, string taskText);

	void SetTaskTextAndWork(string taskName, string taskText, int workValue);

	void IncreaseWorkDoneBy(string taskName, int value);

	void DecreaseWorkDoneBy(string taskName, int value);

	bool IsWorkDone(string taskName);

	bool TaskStarted(string taskName);

	void Done(string taskName);

	bool GetIsCancelled(string taskName);

	void SetIsCancelled(string taskName, bool value);
}
