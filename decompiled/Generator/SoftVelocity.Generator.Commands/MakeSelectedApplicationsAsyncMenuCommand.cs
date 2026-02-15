namespace SoftVelocity.Generator.Commands;

internal class MakeSelectedApplicationsAsyncMenuCommand : AbstractGenerationAsyncMenuCommand
{
	public override void ExecuteApplicationService()
	{
		ApplicationService.MakeSelectedApplications();
	}
}
