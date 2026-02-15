using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class SelectPrevWindow : AbstractMenuCommand
{
	public override void Run()
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
		{
			int num = WorkbenchSingleton.Workbench.ViewContentCollection.IndexOf(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent);
			WorkbenchSingleton.Workbench.ViewContentCollection[(num + WorkbenchSingleton.Workbench.ViewContentCollection.Count - 1) % WorkbenchSingleton.Workbench.ViewContentCollection.Count].WorkbenchWindow.SelectWindow();
		}
	}
}
