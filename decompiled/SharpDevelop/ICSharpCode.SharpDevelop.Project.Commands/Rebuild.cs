namespace ICSharpCode.SharpDevelop.Project.Commands;

public sealed class Rebuild : AbstractBuildMenuCommand
{
	public override void StartBuild()
	{
		StartBuild(BuildTarget.Rebuild);
	}

	public override void AfterBuild()
	{
		ProjectService.RaiseEventEndBuild();
	}
}
