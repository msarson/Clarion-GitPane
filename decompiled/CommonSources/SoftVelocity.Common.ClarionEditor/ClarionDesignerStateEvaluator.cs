using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor;

public class ClarionDesignerStateEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		string text = condition.Properties["state"];
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (WorkbenchSingleton.Workbench == null)
		{
			return false;
		}
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent == null)
		{
			return false;
		}
		if (!(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is CommonClarionDesignerView commonClarionDesignerView))
		{
			return false;
		}
		if (commonClarionDesignerView.IsCompilerResults)
		{
			return false;
		}
		if (text.Equals("report", StringComparison.InvariantCultureIgnoreCase))
		{
			return commonClarionDesignerView.IsReportDesigner;
		}
		if (text.Equals("window", StringComparison.InvariantCultureIgnoreCase))
		{
			return !commonClarionDesignerView.IsReportDesigner;
		}
		return false;
	}
}
