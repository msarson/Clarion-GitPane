using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public class ActiveWindowStateConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent == null)
		{
			return false;
		}
		WindowState windowState = condition.Properties.Get("windowstate", WindowState.None);
		WindowState windowState2 = condition.Properties.Get("nowindowstate", WindowState.None);
		bool flag = false;
		if (windowState != WindowState.None)
		{
			if ((windowState & WindowState.Dirty) > WindowState.None)
			{
				flag |= WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsDirty;
			}
			if ((windowState & WindowState.Untitled) > WindowState.None)
			{
				flag |= WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsUntitled;
			}
			if ((windowState & WindowState.ViewOnly) > WindowState.None)
			{
				flag |= WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsViewOnly;
			}
		}
		else
		{
			flag = true;
		}
		if (windowState2 != WindowState.None)
		{
			if ((windowState2 & WindowState.Dirty) > WindowState.None)
			{
				flag &= !WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsDirty;
			}
			if ((windowState2 & WindowState.Untitled) > WindowState.None)
			{
				flag &= !WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsUntitled;
			}
			if ((windowState2 & WindowState.ViewOnly) > WindowState.None)
			{
				flag &= !WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsViewOnly;
			}
		}
		return flag;
	}
}
