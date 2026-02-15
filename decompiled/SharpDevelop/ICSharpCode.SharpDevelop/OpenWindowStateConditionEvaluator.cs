using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public class OpenWindowStateConditionEvaluator : IConditionEvaluator
{
	private WindowState windowState;

	private WindowState nowindowState;

	private bool IsStateOk(IWorkbenchWindow window)
	{
		if (window == null || window.ViewContent == null)
		{
			return false;
		}
		bool flag = false;
		if (windowState != WindowState.None)
		{
			if ((windowState & WindowState.Dirty) > WindowState.None)
			{
				flag |= window.ViewContent.IsDirty;
			}
			if ((windowState & WindowState.Untitled) > WindowState.None)
			{
				flag |= window.ViewContent.IsUntitled;
			}
			if ((windowState & WindowState.ViewOnly) > WindowState.None)
			{
				flag |= window.ViewContent.IsViewOnly;
			}
		}
		else
		{
			flag = true;
		}
		if (nowindowState != WindowState.None)
		{
			if ((nowindowState & WindowState.Dirty) > WindowState.None)
			{
				flag &= !window.ViewContent.IsDirty;
			}
			if ((nowindowState & WindowState.Untitled) > WindowState.None)
			{
				flag &= !window.ViewContent.IsUntitled;
			}
			if ((nowindowState & WindowState.ViewOnly) > WindowState.None)
			{
				flag &= !window.ViewContent.IsViewOnly;
			}
		}
		return flag;
	}

	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench == null)
		{
			return false;
		}
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent == null)
		{
			return false;
		}
		windowState = condition.Properties.Get("openwindowstate", WindowState.None);
		nowindowState = condition.Properties.Get("noopenwindowstate", WindowState.None);
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			if (IsStateOk(item.WorkbenchWindow))
			{
				return true;
			}
		}
		return false;
	}
}
