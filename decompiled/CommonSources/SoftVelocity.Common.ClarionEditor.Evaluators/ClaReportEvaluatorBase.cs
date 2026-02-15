using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.Evaluators;

public class ClaReportEvaluatorBase : IConditionEvaluator
{
	public CommonClarionDesignerView View => WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as CommonClarionDesignerView;

	public virtual bool IsValid(object caller, Condition condition)
	{
		return false;
	}
}
