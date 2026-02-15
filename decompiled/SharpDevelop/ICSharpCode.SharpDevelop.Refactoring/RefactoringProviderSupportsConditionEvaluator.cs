using System;
using System.Reflection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Refactoring;

public class RefactoringProviderSupportsConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench == null || WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == null)
		{
			return false;
		}
		if (!(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is ITextEditorControlProvider textEditorControlProvider))
		{
			return false;
		}
		LanguageProperties language = ParserService.CurrentProjectContent.Language;
		if (language == null)
		{
			return false;
		}
		if (textEditorControlProvider.TextEditorControl == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(textEditorControlProvider.TextEditorControl.FileName))
		{
			return false;
		}
		RefactoringProvider refactoringProvider = language.RefactoringProvider;
		if (!refactoringProvider.IsEnabledForFile(textEditorControlProvider.TextEditorControl.FileName))
		{
			return false;
		}
		string text = condition.Properties["supports"];
		if (text == "*")
		{
			return true;
		}
		Type type = refactoringProvider.GetType();
		try
		{
			return (bool)type.InvokeMember("Supports" + text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty, null, refactoringProvider, null);
		}
		catch (Exception ex)
		{
			LoggingService.Warn(ex.ToString());
			return false;
		}
	}
}
