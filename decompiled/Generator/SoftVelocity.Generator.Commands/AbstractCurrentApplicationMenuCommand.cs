using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator.UI;

namespace SoftVelocity.Generator.Commands;

internal abstract class AbstractCurrentApplicationMenuCommand : AbstractMenuCommand
{
	protected Application _app;

	public override bool IsEnabled
	{
		get
		{
			if (ApplicationService.ApplicationsList.Count > 0 && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is ApplicationMainWindowControl_ViewContent applicationMainWindowControl_ViewContent)
			{
				_app = applicationMainWindowControl_ViewContent.App;
				return true;
			}
			_app = null;
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
			DoRun(_app);
		}
	}

	public abstract void DoRun(Application app);
}
