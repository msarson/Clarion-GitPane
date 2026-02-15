using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator.UI;

namespace SoftVelocity.Generator.Commands;

internal class OpenApplicationDictionaryMenuCommand : AbstractMenuCommand
{
	protected Application app;

	public override bool IsEnabled
	{
		get
		{
			IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
			if (activeWorkbenchWindow == null)
			{
				return false;
			}
			if (activeWorkbenchWindow.ViewContent is ApplicationMainWindowControl_ViewContent applicationMainWindowControl_ViewContent)
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

	public override void Run()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			ApplicationService.OpenDictionary(app);
		}
	}
}
