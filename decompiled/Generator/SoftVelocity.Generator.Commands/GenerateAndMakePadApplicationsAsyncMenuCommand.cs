using ICSharpCode.Core;
using SoftVelocity.Generator.UI;

namespace SoftVelocity.Generator.Commands;

internal class GenerateAndMakePadApplicationsAsyncMenuCommand : AbstractGenerationAsyncMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return ApplicationService.ApplicationsList.Count > 0;
		}
		set
		{
		}
	}

	public override void ExecuteApplicationService()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			ApplicationBrowserPad.Instance.Applications.PressBuildButton();
		}
	}
}
