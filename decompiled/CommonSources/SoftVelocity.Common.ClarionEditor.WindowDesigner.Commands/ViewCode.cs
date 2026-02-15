using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class ViewCode : AbstractMenuCommand
{
	public CommonClarionDesignerView View
	{
		get
		{
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null)
			{
				return null;
			}
			return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as CommonClarionDesignerView;
		}
	}

	protected FormsDesignerViewContent FormDesigner => View;

	public override void Run()
	{
		((WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null) ? null : (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as IBackToSourceCompatible))?.BackToSource();
	}
}
