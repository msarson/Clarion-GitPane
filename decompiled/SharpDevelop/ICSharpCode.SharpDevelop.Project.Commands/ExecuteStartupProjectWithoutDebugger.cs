namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ExecuteStartupProjectWithoutDebugger : ExecuteStartupProject
{
	public override void Run()
	{
		withDebugger = false;
		base.Run();
	}
}
