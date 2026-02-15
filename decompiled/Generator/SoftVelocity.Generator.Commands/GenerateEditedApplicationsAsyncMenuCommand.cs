using ICSharpCode.Core;

namespace SoftVelocity.Generator.Commands;

internal class GenerateEditedApplicationsAsyncMenuCommand : AbstractGenerationAsyncMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return ApplicationService.ApplicationsLoaded.Count > 0;
		}
		set
		{
		}
	}

	public override void ExecuteApplicationService()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			ApplicationService.GenerateAllEditedApplications();
		}
	}
}
