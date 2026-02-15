using System;

namespace ICSharpCode.SharpDevelop;

public class TaskEventArgs : EventArgs
{
	private Task task;

	public Task Task => task;

	public TaskEventArgs(Task task)
	{
		this.task = task;
	}
}
