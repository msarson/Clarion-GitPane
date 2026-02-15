using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ExecuteStartupProject : AbstractMenuCommand
{
	protected bool withDebugger = true;

	public override void Run()
	{
		Build build = new Build();
		build.BuildComplete += delegate
		{
			if (build.LastBuildResults.ErrorCount == 0)
			{
				IProject startupProject = ProjectService.OpenSolution.StartupProject;
				if (startupProject != null)
				{
					startupProject.Start(withDebugger);
				}
				else
				{
					MessageService.ShowError("${res:BackendBindings.ExecutionManager.CantExecuteDLLError}");
				}
			}
		};
		build.Run();
	}
}
