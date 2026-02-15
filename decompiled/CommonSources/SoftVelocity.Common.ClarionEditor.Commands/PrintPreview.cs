using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class PrintPreview : AbstractClarionDesignerCommand
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

	public override void Run()
	{
		((WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null) ? null : (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as CommonClarionDesignerView))?.PrintPreview();
	}
}
