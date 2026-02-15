using System;
using ICSharpCode.Core;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Conditions;

public class TextContentConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		string a = condition.Properties["textcontent"];
		if (caller is TextEditorControl)
		{
			TextEditorControl textEditorControl = (TextEditorControl)caller;
			if (textEditorControl.Document != null && textEditorControl.Document.HighlightingStrategy != null)
			{
				return string.Equals(a, textEditorControl.Document.HighlightingStrategy.Name, StringComparison.OrdinalIgnoreCase);
			}
		}
		return false;
	}
}
