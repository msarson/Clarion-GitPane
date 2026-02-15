using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class ShowSensitiveHelp : AbstractMenuCommand
{
	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		IContextHelpProvider contextHelpProvider = ((activeWorkbenchWindow != null) ? (activeWorkbenchWindow.ActiveViewContent as IContextHelpProvider) : null);
		foreach (PadDescriptor item in WorkbenchSingleton.Workbench.PadContentCollection)
		{
			if (item.HasFocus && item.PadContent is IContextHelpProvider)
			{
				((IContextHelpProvider)item.PadContent).ShowHelp();
				return;
			}
		}
		contextHelpProvider?.ShowHelp();
	}
}
