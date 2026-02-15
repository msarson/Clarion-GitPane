using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public class WindowOpenConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench == null)
		{
			return false;
		}
		string text = condition.Properties["openwindow"];
		if (text == "*")
		{
			return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null;
		}
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			Type type = item.GetType();
			if (type.ToString() == text)
			{
				return true;
			}
			Type[] interfaces = type.GetInterfaces();
			foreach (Type type2 in interfaces)
			{
				if (type2.ToString() == text)
				{
					return true;
				}
			}
		}
		return false;
	}
}
