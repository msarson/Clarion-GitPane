using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ExecuteCurrentProject : AbstractMenuCommand
{
	protected bool withDebugger = true;

	public override void Run()
	{
		if (VersionService.Version == IDEVersion.Enterprise)
		{
			IProject currentProject = ProjectService.CurrentProject;
			BuildProject build = new BuildProject(currentProject);
			build.AdditionalProperties.Add("NoDependency", "true");
			build.BuildComplete += delegate
			{
				if (build.LastBuildResults.ErrorCount == 0)
				{
					if (currentProject != null)
					{
						currentProject.Start(withDebugger);
					}
					else
					{
						MessageService.ShowError("${res:BackendBindings.ExecutionManager.CantExecuteDLLError}");
					}
				}
			};
			build.Run();
			return;
		}
		Build build2 = new Build();
		build2.BuildComplete += delegate
		{
			if (build2.LastBuildResults.ErrorCount == 0)
			{
				IProject currentProject2 = ProjectService.CurrentProject;
				if (currentProject2 != null)
				{
					currentProject2.Start(withDebugger);
				}
				else
				{
					MessageService.ShowError("${res:BackendBindings.ExecutionManager.CantExecuteDLLError}");
				}
			}
		};
		build2.Run();
	}
}
