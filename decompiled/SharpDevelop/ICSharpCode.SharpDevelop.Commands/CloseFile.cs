using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class CloseFile : AbstractMenuCommand
{
	public override void Run()
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
		{
			WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.CloseWindow(force: false);
		}
	}
}
