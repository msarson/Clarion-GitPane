namespace ICSharpCode.SharpDevelop.Project.Commands;

public sealed class Build : AbstractBuildMenuCommand
{
	public override void StartBuild()
	{
		StartBuild(BuildTarget.Build);
	}

	public override void AfterBuild()
	{
		ProjectService.RaiseEventEndBuild();
	}
}
