namespace SoftVelocity.Generator.Commands;

public class EditSelectedApplicationsInSolutionMenuCommand : ApplicationsInSolutionMenuCommand
{
	protected override void ExecuteApplicationService()
	{
		ApplicationService.EditApplication(app);
	}
}
