namespace ICSharpCode.SharpDevelop.Project;

public class ProjectItemEventArgs : ProjectEventArgs
{
	private ProjectItem projectItem;

	public ProjectItem ProjectItem => projectItem;

	public ProjectItemEventArgs(IProject project, ProjectItem projectItem)
		: base(project)
	{
		this.projectItem = projectItem;
	}
}
