using System;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class NewTaskEventArgs : EventArgs
{
	private Task task;

	private BuildError error;

	public Task Task
	{
		get
		{
			return task;
		}
		set
		{
			task = value;
		}
	}

	public BuildError BuildError => error;

	public NewTaskEventArgs(BuildError error)
	{
		this.error = error;
		task = null;
	}
}
