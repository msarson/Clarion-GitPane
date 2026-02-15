namespace ICSharpCode.SharpDevelop.Project.Commands;

public sealed class Clean : AbstractBuildMenuCommand
{
	public override void StartBuild()
	{
		StartBuild(BuildTarget.Clean);
	}

	public override void AfterBuild()
	{
		ProjectService.RaiseEventEndBuild();
	}
}
