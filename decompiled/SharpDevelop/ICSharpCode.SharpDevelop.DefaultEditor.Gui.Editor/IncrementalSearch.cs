using System;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class IncrementalSearch : IDisposable
{
	private class IncrementalSearchFormattingStrategy : DefaultFormattingStrategy
	{
		public override void FormatLine(TextArea textArea, int line, int cursorOffset, char ch)
		{
		}
	}

	private bool disposed;

	private TextEditorControl textEditor;

	private Cursor previousCursor;

	private IFormattingStrategy previousFormattingStrategy;

	private string incrementalSearchStartMessage;

	private StringBuilder searchText = new StringBuilder();

	private string text;

	private int startIndex;

	private int originalStartIndex;

	private Cursor cursor;

	private bool passedEndOfDocument;

	private bool codeCompletionEnabled;

	private bool forwards = true;

	private TextArea TextArea => textEditor.ActiveTextAreaControl.TextArea;

	public IncrementalSearch(TextEditorControl textEditor)
		: this(textEditor, forwards: true)
	{
	}

	public IncrementalSearch(TextEditorControl textEditor, bool forwards)
	{
		this.forwards = forwards;
		if (forwards)
		{
			incrementalSearchStartMessage = StringParser.Parse("${res:ICSharpCode.SharpDevelop.DefaultEditor.IncrementalSearch.ForwardsSearchStatusBarMessage} ");
		}
		else
		{
			incrementalSearchStartMessage = StringParser.Parse("${res:ICSharpCode.SharpDevelop.DefaultEditor.IncrementalSearch.ReverseSearchStatusBarMessage} ");
		}
		this.textEditor = textEditor;
		codeCompletionEnabled = CodeCompletionOptions.EnableCodeCompletion;
		CodeCompletionOptions.EnableCodeCompletion = false;
		AddFormattingStrategy();
		TextArea.IncrementalSearchKeyEventHandler += TextAreaKeyPress;
		TextArea.DoProcessDialogKey += TextAreaProcessDialogKey;
		TextArea.LostFocus += TextAreaLostFocus;
		TextArea.MouseClick += TextAreaMouseClick;
		EnableIncrementalSearchCursor();
		text = textEditor.Document.TextContent;
		startIndex = TextArea.Caret.Offset;
		originalStartIndex = startIndex;
		GetInitialSearchText();
		ShowTextFound(searchText.ToString());
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			TextArea.IncrementalSearchKeyEventHandler -= TextAreaKeyPress;
			TextArea.DoProcessDialogKey -= TextAreaProcessDialogKey;
			TextArea.LostFocus -= TextAreaLostFocus;
			TextArea.MouseClick -= TextAreaMouseClick;
			DisableIncrementalSearchCursor();
			RemoveFormattingStrategy();
			if (cursor != null)
			{
				cursor.Dispose();
			}
			ClearStatusBarMessage();
		}
	}

	private void TextAreaLostFocus(object source, EventArgs e)
	{
		StopIncrementalSearch();
	}

	private void TextAreaMouseClick(object source, MouseEventArgs e)
	{
		StopIncrementalSearch();
	}

	private void StopIncrementalSearch()
	{
		CodeCompletionOptions.EnableCodeCompletion = codeCompletionEnabled;
		Dispose();
	}

	private bool TextAreaKeyPress(char ch)
	{
		searchText.Append(ch);
		RunSearch();
		return true;
	}

	private void HighlightText(int offset, int length)
	{
		int offset2 = offset + length;
		TextArea.Caret.Position = TextArea.Document.OffsetToPosition(offset2);
		TextArea.SelectionManager.ClearSelection();
		IDocument document = TextArea.Document;
		DefaultSelection selection = new DefaultSelection(document, document.OffsetToPosition(offset), document.OffsetToPosition(offset2));
		TextArea.SelectionManager.SetSelection(selection);
		textEditor.Refresh();
	}

	private void RunSearch()
	{
		string text = searchText.ToString();
		int num = FindText(text, startIndex, forwards);
		if (num == -1)
		{
			num = FindText(text, GetWrapAroundStartIndex(), forwards);
			passedEndOfDocument = true;
		}
		if (num >= 0)
		{
			startIndex = num;
			HighlightText(num, text.Length);
			ShowTextFound(text);
		}
		else
		{
			ShowTextNotFound(text);
		}
	}

	private int GetWrapAroundStartIndex()
	{
		int result = 0;
		if (!forwards)
		{
			result = text.Length - 1;
		}
		return result;
	}

	private int FindText(string find, int startIndex, bool forwards)
	{
		StringComparison stringComparisonType = GetStringComparisonType(find);
		if (forwards)
		{
			return text.IndexOf(find, startIndex, stringComparisonType);
		}
		string reverseSearchText = GetReverseSearchText(startIndex + find.Length);
		return reverseSearchText.LastIndexOf(find, stringComparisonType);
	}

	private StringComparison GetStringComparisonType(string find)
	{
		foreach (char c in find)
		{
			if (char.IsUpper(c))
			{
				return StringComparison.InvariantCulture;
			}
		}
		return StringComparison.InvariantCultureIgnoreCase;
	}

	private string GetReverseSearchText(int endIndex)
	{
		if (endIndex < text.Length)
		{
			return text.Substring(0, endIndex);
		}
		endIndex = text.Length - 1;
		if (endIndex >= 0)
		{
			return text;
		}
		return string.Empty;
	}

	private bool TextAreaProcessDialogKey(Keys keys)
	{
		switch (keys)
		{
		case Keys.Return:
		case Keys.Escape:
			StopIncrementalSearch();
			return true;
		case Keys.Back:
		{
			int length = searchText.ToString().Length;
			if (length > 0)
			{
				searchText.Remove(length - 1, 1);
				startIndex = originalStartIndex;
				passedEndOfDocument = false;
				RunSearch();
				return true;
			}
			StopIncrementalSearch();
			return false;
		}
		default:
			if (textEditor.IsEditAction(keys))
			{
				StopIncrementalSearch();
				return false;
			}
			return false;
		}
	}

	private static bool IsGreaterThanKey(Keys keys)
	{
		return (keys & Keys.KeyCode) == (Keys.IMEAccept | Keys.Space);
	}

	private void ShowTextFound(string find)
	{
		if (passedEndOfDocument)
		{
			ShowMessage(find + StringParser.Parse(" ${res:ICSharpCode.SharpDevelop.DefaultEditor.IncrementalSearch.PassedEndOfDocumentStatusBarMessage}"), highlight: true);
		}
		else
		{
			ShowMessage(find, highlight: false);
		}
	}

	private void ShowMessage(string message, bool highlight)
	{
		string message2 = incrementalSearchStartMessage + message;
		StatusBarService.SetMessage(message2, highlight);
	}

	private void ShowTextNotFound(string find)
	{
		ShowMessage(find + StringParser.Parse(" ${res:ICSharpCode.SharpDevelop.DefaultEditor.IncrementalSearch.NotFoundStatusBarMessage}"), highlight: true);
	}

	private void ClearStatusBarMessage()
	{
		StatusBarService.SetMessage(string.Empty);
	}

	private void GetInitialSearchText()
	{
		if (TextArea.SelectionManager.HasSomethingSelected)
		{
			ISelection selection = TextArea.SelectionManager.SelectionCollection[0];
			startIndex = selection.Offset;
			if (!IsMultilineSelection(selection))
			{
				searchText.Append(selection.SelectedText);
			}
		}
	}

	private bool IsMultilineSelection(ISelection selection)
	{
		return selection.StartPosition.Y != selection.EndPosition.Y;
	}

	private Cursor GetCursor()
	{
		if (cursor == null)
		{
			string name = "Resources.IncrementalSearchCursor.cur";
			if (!forwards)
			{
				name = "Resources.ReverseIncrementalSearchCursor.cur";
			}
			cursor = new Cursor(GetType().Assembly.GetManifestResourceStream(name));
		}
		return cursor;
	}

	private void EnableIncrementalSearchCursor()
	{
		previousCursor = TextArea.Cursor;
		Cursor cursor = GetCursor();
		TextArea.Cursor = cursor;
		TextArea.TextView.Cursor = cursor;
	}

	private void DisableIncrementalSearchCursor()
	{
		TextArea.Cursor = previousCursor;
		TextArea.TextView.Cursor = previousCursor;
	}

	private void AddFormattingStrategy()
	{
		IDocument document = textEditor.Document;
		previousFormattingStrategy = document.FormattingStrategy;
		textEditor.Document.FormattingStrategy = new IncrementalSearchFormattingStrategy();
	}

	private void RemoveFormattingStrategy()
	{
		textEditor.Document.FormattingStrategy = previousFormattingStrategy;
	}
}
