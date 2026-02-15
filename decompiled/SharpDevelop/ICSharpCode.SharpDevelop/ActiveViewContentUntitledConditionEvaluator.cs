using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public class ActiveViewContentUntitledConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent == null)
		{
			return false;
		}
		if (!condition.Properties.Contains("activewindowuntitled"))
		{
			return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsUntitled;
		}
		bool flag = bool.Parse(condition.Properties["activewindowuntitled"]);
		return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsUntitled == flag;
	}
}
