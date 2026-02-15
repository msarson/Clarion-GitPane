using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor;

public class ClaCanCallDesignerEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench == null)
		{
			return false;
		}
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent == null)
		{
			return false;
		}
		if (!(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is IStructureDesignerCompatible structureDesignerCompatible))
		{
			return false;
		}
		return structureDesignerCompatible.CanShowStructureDesigner;
	}
}
