using System;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using SearchAndReplace;

namespace SoftVelocity.Generator.TemplateRegistry.Actions;

public abstract class BaseSearchSymbolAction : BaseSymbolSearchRedFileAction
{
	protected override void ExecuteSearch(string symbolToSearch, TextArea textArea)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		int column = textArea.Caret.Column;
		int line = textArea.Caret.Line;
		textArea.Caret.Column = 0;
		textArea.Caret.Line = 0;
		ProgressNotificationTaskInstance val = new ProgressNotificationTaskInstance("Searching %" + symbolToSearch, true);
		try
		{
			SearchInFilesManager.FindAll((IProgressNotificationTaskInstance)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		textArea.Caret.Column = column;
		textArea.Caret.Line = line;
	}
}
