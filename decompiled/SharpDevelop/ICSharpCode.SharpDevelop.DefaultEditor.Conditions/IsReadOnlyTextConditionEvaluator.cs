using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Conditions;

public class IsReadOnlyTextConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench != null && WorkbenchSingleton.Workbench.ActiveContent is ITextEditorControlProvider)
		{
			ITextEditorControlProvider textEditorControlProvider = (ITextEditorControlProvider)WorkbenchSingleton.Workbench.ActiveContent;
			if (textEditorControlProvider.TextEditorControl != null && textEditorControlProvider.TextEditorControl.ActiveTextAreaControl != null)
			{
				int line = textEditorControlProvider.TextEditorControl.ActiveTextAreaControl.Caret.Line;
				if (textEditorControlProvider.TextEditorControl.ActiveTextAreaControl.Document.CustomLineManager.IsReadOnly(line, defaultReadOnly: false))
				{
					return true;
				}
			}
		}
		return false;
	}
}
