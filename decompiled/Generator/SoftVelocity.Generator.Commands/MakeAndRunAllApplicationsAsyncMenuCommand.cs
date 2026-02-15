using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.Commands;

internal class MakeAndRunAllApplicationsAsyncMenuCommand : AbstractGenerationAsyncMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return ProjectService.OpenSolution != null;
		}
		set
		{
		}
	}

	public override void ExecuteApplicationService()
	{
		ApplicationService.MakeAndRunAllApplications();
	}
}
