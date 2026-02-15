using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public abstract class AbstractClarionDesignerCommand : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
			{
				return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is CommonClarionDesignerView;
			}
			return false;
		}
		set
		{
		}
	}
}
