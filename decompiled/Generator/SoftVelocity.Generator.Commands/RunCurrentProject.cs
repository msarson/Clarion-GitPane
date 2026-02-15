using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.Commands;

internal class RunCurrentProject : AbstractRunProjectMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return ProjectService.CurrentProject != null;
		}
		set
		{
		}
	}

	public override void Run()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			DoRun(ProjectService.CurrentProject);
		}
	}
}
