using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator.UI;

namespace SoftVelocity.Generator.Conditions;

public class CanGenerateCurrentApplication : IConditionEvaluator
{
	public static bool IsValid()
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent == null)
		{
			return false;
		}
		if (((object)WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent).GetType() != typeof(ApplicationMainWindowControl_ViewContent))
		{
			return false;
		}
		if (ApplicationService.Instance.Generating || ApplicationService.IsTemplateRegistryOpen || ApplicationService.ApplicationsList.Count == 0)
		{
			return false;
		}
		return true;
	}

	public bool IsValid(object caller, Condition condition)
	{
		return IsValid();
	}
}
