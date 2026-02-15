using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class NavigateNextWindow : AbstractMenuCommand
{
	public override void Run()
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null || WorkbenchSingleton.Workbench.PadContentCollection.FindAll(PadsNavigationDialog.VisiblePad).Count > 0)
		{
			int selectedViewIndex = 0;
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
			{
				int num = WorkbenchSingleton.Workbench.ViewContentCollection.IndexOf(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent);
				selectedViewIndex = (num + 1) % WorkbenchSingleton.Workbench.ViewContentCollection.Count;
			}
			using PadsNavigationDialog padsNavigationDialog = new PadsNavigationDialog(selectedViewIndex);
			padsNavigationDialog.ShowDialog(WorkbenchSingleton.MainForm);
			padsNavigationDialog.ExecAction();
		}
	}
}
