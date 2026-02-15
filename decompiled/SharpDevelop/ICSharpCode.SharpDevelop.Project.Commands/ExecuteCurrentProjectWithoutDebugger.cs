namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ExecuteCurrentProjectWithoutDebugger : ExecuteCurrentProject
{
	public override void Run()
	{
		withDebugger = false;
		base.Run();
	}
}
