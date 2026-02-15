using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class DesignerAbstractCheckableMenuCommand : AbstractCheckableMenuCommand
{
	private bool isEnabled = true;

	public override bool IsEnabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			isEnabled = value;
		}
	}

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

	private FormsDesignerViewContent FormDesigner => View;
}
