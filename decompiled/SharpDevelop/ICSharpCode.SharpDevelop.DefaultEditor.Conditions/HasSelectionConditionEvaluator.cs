using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Conditions;

public class HasSelectionConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		if (WorkbenchSingleton.Workbench != null && WorkbenchSingleton.Workbench.ActiveContent is ITextEditorControlProvider)
		{
			ITextEditorControlProvider textEditorControlProvider = (ITextEditorControlProvider)WorkbenchSingleton.Workbench.ActiveContent;
			if (textEditorControlProvider.TextEditorControl != null && textEditorControlProvider.TextEditorControl.ActiveTextAreaControl != null && textEditorControlProvider.TextEditorControl.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
			{
				return true;
			}
		}
		return false;
	}
}
