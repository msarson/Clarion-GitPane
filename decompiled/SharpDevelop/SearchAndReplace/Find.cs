using ICSharpCode.Core;
using ICSharpCode.TextEditor;

namespace SearchAndReplace;

public class Find : AbstractMenuCommand
{
	public static void SetSearchPattern()
	{
		TextEditorControl activeTextEditor = SearchReplaceUtilities.GetActiveTextEditor();
		if (activeTextEditor != null)
		{
			string selectedText = activeTextEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
			if (selectedText != null && selectedText.Length > 0 && !IsMultipleLines(selectedText))
			{
				SearchOptions.CurrentFindPattern = selectedText;
			}
		}
	}

	public override void Run()
	{
		SetSearchPattern();
		SearchAndReplaceDialog.ShowSingleInstance(SearchAndReplaceMode.Search);
	}

	public static bool IsMultipleLines(string text)
	{
		return text.IndexOf('\n') != -1;
	}
}
