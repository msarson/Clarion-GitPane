using System;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public class ActiveContentExtensionConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent == null)
		{
			return false;
		}
		try
		{
			string text = (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsUntitled ? WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.UntitledName : WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName);
			if (text == null)
			{
				return false;
			}
			string extension = Path.GetExtension(text);
			return extension.ToUpperInvariant() == condition.Properties["activeextension"].ToUpperInvariant();
		}
		catch (Exception)
		{
			return false;
		}
	}
}
