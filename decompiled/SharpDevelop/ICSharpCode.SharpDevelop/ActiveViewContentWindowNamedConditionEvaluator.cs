using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public class ActiveViewContentWindowNamedConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent == null)
		{
			return false;
		}
		return !string.IsNullOrEmpty(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName);
	}
}
