namespace ICSharpCode.SharpDevelop.Gui;

public interface IProgressNotificationTask
{
	string TaskText { get; set; }

	int WorkDone { get; set; }

	bool IsWorkDone { get; }

	string TaskName { get; set; }

	bool IsCancelled { get; set; }

	void BeginTask(string name, int totalWork, bool allowCancel);

	void BeginTask(string taskName, string initialTaskText, int totalWork, bool allowCancel);

	void Done();

	void Show();
}
