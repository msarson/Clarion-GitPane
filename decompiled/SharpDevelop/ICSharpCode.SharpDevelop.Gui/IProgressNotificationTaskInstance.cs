namespace ICSharpCode.SharpDevelop.Gui;

public interface IProgressNotificationTaskInstance
{
	int WorkDone { get; set; }

	string TaskText { set; }

	bool IsWorkDone { get; }

	bool TaskStarted { get; }

	bool IsCancelled { get; set; }

	void BeginTask(string initialTaskText, int totalWork, bool allowCancel);

	void IncreseWorkDoneBy(int value);

	void DecreseWorkDoneBy(int value);

	void Done();
}
