namespace ICSharpCode.SharpDevelop.Project.Commands;

public class CleanProject : BuildProject
{
	public override void StartBuild()
	{
		StartBuild(BuildTarget.Clean);
	}
}
