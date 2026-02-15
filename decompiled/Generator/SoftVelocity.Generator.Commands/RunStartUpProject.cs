using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.Commands;

internal class RunStartUpProject : AbstractRunProjectMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (ProjectService.OpenSolution != null)
			{
				return ProjectService.OpenSolution.StartupProject != null;
			}
			return false;
		}
		set
		{
		}
	}

	public override void Run()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			base.FallbackToStartUp = false;
			DoRun(ProjectService.OpenSolution.StartupProject);
		}
	}
}
