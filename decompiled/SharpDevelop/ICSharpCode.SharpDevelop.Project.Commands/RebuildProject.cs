namespace ICSharpCode.SharpDevelop.Project.Commands;

public sealed class RebuildProject : BuildProject
{
	public RebuildProject()
	{
	}

	public RebuildProject(IProject targetProject)
		: base(targetProject)
	{
	}

	public override void StartBuild()
	{
		StartBuild(BuildTarget.Rebuild);
	}
}
