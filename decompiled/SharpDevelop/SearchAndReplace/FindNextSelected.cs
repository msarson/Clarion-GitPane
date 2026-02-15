using ICSharpCode.Core;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class FindNextSelected : AbstractMenuCommand
{
	public override void Run()
	{
		TextEditorControl activeTextEditor = SearchReplaceUtilities.GetActiveTextEditor();
		if (activeTextEditor == null)
		{
			return;
		}
		string selectedText = activeTextEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
		string text = ((selectedText.Length <= 0) ? TextUtilities.GetWordAt(activeTextEditor.Document, activeTextEditor.ActiveTextAreaControl.Caret.Offset) : ((!Find.IsMultipleLines(selectedText)) ? selectedText : TextUtilities.GetWordAt(activeTextEditor.Document, activeTextEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectionCollection[0].Offset)));
		if (text != null && text.Length > 0)
		{
			SearchOptions.CurrentFindPattern = text;
			if (SearchOptions.SearchAndReplaceBinding == SearchOptions.CurrentSelectionBinding)
			{
				SearchOptions.SearchAndReplaceBinding = SearchOptions.CurrentDocumentBinding;
			}
			SearchReplaceManager.FindNext(null);
		}
	}
}
