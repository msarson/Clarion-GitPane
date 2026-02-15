using System;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class ProjectConfigurationEventArgs : EventArgs
{
	private string configuration;

	private IProject project;

	public string Configuration => configuration;

	public IProject Project => project;

	public ProjectConfigurationEventArgs(IProject project, string configuration)
	{
		this.configuration = configuration;
		this.project = project;
	}
}
