using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class ViewTabOrder : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return FormDesigner?.IsTabOrderMode ?? false;
		}
		set
		{
			SetTabOrder(value);
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

	private void SetTabOrder(bool show)
	{
		FormsDesignerViewContent formDesigner = FormDesigner;
		if (formDesigner != null)
		{
			if (show)
			{
				View.StartSetTabOrder();
				formDesigner.ShowTabOrder();
			}
			else
			{
				View.ResetTabOrder();
				formDesigner.HideTabOrder();
			}
		}
	}
}
