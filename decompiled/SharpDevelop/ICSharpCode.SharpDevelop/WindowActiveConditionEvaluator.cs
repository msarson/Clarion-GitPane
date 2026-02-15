using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public class WindowActiveConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench == null)
		{
			return false;
		}
		string text = condition.Properties["activewindow"];
		if (text == "*")
		{
			return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null;
		}
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent == null)
		{
			return false;
		}
		Type type = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent.GetType();
		if (type.FullName == text)
		{
			return true;
		}
		Type[] interfaces = type.GetInterfaces();
		foreach (Type type2 in interfaces)
		{
			if (type2.FullName == text)
			{
				return true;
			}
		}
		while ((type = type.BaseType) != null)
		{
			if (type.FullName == text)
			{
				return true;
			}
		}
		return false;
	}
}
