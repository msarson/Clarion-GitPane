using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator.Conditions;
using SoftVelocity.Generator.UI;

namespace SoftVelocity.Generator.Commands;

internal class GenerateCurrentApplicationAsyncMenuCommand : AbstractGenerationAsyncMenuCommand
{
	protected Application app;

	public override bool IsEnabled
	{
		get
		{
			if (CanGenerateCurrentApplication.IsValid() && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is ApplicationMainWindowControl_ViewContent applicationMainWindowControl_ViewContent)
			{
				app = applicationMainWindowControl_ViewContent.App;
				return true;
			}
			app = null;
			return false;
		}
		set
		{
		}
	}

	public override void ExecuteApplicationService()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			ApplicationService.GenerateApplication(app);
		}
	}
}
