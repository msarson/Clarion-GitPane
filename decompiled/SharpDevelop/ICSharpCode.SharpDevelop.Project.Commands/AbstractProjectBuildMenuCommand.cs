namespace ICSharpCode.SharpDevelop.Project.Commands;

public abstract class AbstractProjectBuildMenuCommand : AbstractBuildMenuCommand
{
	protected IProject targetProject;

	public string ProjectFileName
	{
		get
		{
			if (ProjectToBuild != null)
			{
				return ProjectToBuild.FileName;
			}
			return string.Empty;
		}
	}

	protected IProject ProjectToBuild => targetProject ?? ProjectService.CurrentProject;

	public override bool CanRunBuild
	{
		get
		{
			if (base.CanRunBuild)
			{
				return ProjectToBuild != null;
			}
			return false;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			targetProject = null;
		}
		base.Dispose(disposing);
	}
}
