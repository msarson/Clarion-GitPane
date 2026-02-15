using System;

namespace ICSharpCode.SharpDevelop.Project;

public class ProjectEventArgs : EventArgs
{
	private IProject project;

	public IProject Project => project;

	public ProjectEventArgs(IProject project)
	{
		this.project = project;
	}
}
