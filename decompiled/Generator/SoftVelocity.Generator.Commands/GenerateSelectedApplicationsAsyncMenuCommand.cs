namespace SoftVelocity.Generator.Commands;

internal class GenerateSelectedApplicationsAsyncMenuCommand : AbstractGenerationAsyncMenuCommand
{
	public override void ExecuteApplicationService()
	{
		ApplicationService.GenerateSelectedApplications();
	}
}
