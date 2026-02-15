using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.Commands;

public class GenerateAllApplicationsInSolutionMenuCommand : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (ApplicationService.ApplicationsList.Count > 0)
			{
				return true;
			}
			return false;
		}
		set
		{
		}
	}

	public override void Run()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		WorkbenchSingleton.SafeThreadAsyncCall(new Action(ExecuteApplicationService));
	}

	public void ExecuteApplicationService()
	{
		ApplicationService.GenerateAllApplications();
	}
}
