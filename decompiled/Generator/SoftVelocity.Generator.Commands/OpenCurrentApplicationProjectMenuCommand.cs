using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Project.Commands;

namespace SoftVelocity.Generator.Commands;

internal class OpenCurrentApplicationProjectMenuCommand : AbstractCurrentApplicationMenuCommand
{
	public static void OpenApplicationProject(Application app)
	{
		if (app == null)
		{
			return;
		}
		string projectFileName = app.ProjectFileName;
		if (!string.IsNullOrEmpty(projectFileName))
		{
			IProject project = ProjectService.GetProject(app.ProjectFileName);
			if (project != null)
			{
				ViewProjectOptions.ShowProjectOptions(project);
			}
		}
	}

	public override void DoRun(Application app)
	{
		OpenApplicationProject(app);
	}
}
