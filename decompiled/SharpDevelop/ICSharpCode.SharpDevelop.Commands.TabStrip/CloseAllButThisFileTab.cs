using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands.TabStrip;

public class CloseAllButThisFileTab : AbtractWorkbenchWindowMenuCommand
{
	public override void Run()
	{
		if (!IsEnabled)
		{
			return;
		}
		IViewContent viewContent = null;
		int num = 0;
		while (num < WorkbenchSingleton.Workbench.ViewContentCollection.Count)
		{
			IViewContent viewContent2 = WorkbenchSingleton.Workbench.ViewContentCollection[num];
			if (viewContent2.WorkbenchWindow != base.Window && viewContent2 != viewContent)
			{
				viewContent2.WorkbenchWindow.CloseWindow(force: false);
				viewContent = viewContent2;
			}
			else
			{
				num++;
			}
		}
	}
}
