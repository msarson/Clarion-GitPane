using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator.UI;

namespace SoftVelocity.Generator.Conditions;

public class CurrentViewIsApplication : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent == null)
		{
			return false;
		}
		if (((object)WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent).GetType() == typeof(ApplicationMainWindowControl_ViewContent))
		{
			return true;
		}
		return false;
	}
}
