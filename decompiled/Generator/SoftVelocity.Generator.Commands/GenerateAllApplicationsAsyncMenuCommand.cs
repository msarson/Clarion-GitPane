namespace SoftVelocity.Generator.Commands;

internal class GenerateAllApplicationsAsyncMenuCommand : AbstractGenerationAsyncMenuCommand
{
	public override void ExecuteApplicationService()
	{
		ApplicationService.GenerateAllApplications();
	}
}
