using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

internal class CurrentSelectionSearcher : ISearcher, IDisposable
{
	private TextEditorControl textEditor;

	private ISelection selection;

	private bool ignoreSelectionChanges;

	private bool findFirst;

	public EventHandler ActiveChanged;

	internal bool Active => IsTextSelected(selection);

	public CurrentSelectionSearcher()
	{
		selection = GetCurrentTextSelection();
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += WorkbenchWindowChanged;
		AddSelectionChangedHandler(SearchReplaceUtilities.GetActiveTextEditor());
	}

	public void Dispose()
	{
		RemoveSelectionChangedHandler();
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged -= WorkbenchWindowChanged;
	}

	public void RunAll(SearchType action, IProgressNotificationTaskInstance monitor)
	{
		int num = Math.Min(selection.Offset, selection.EndOffset);
		int num2 = Math.Max(selection.Offset, selection.EndOffset);
		SearchReplaceUtilities.SelectText(textEditor, num, num2);
		SetCaretPosition(textEditor.ActiveTextAreaControl.TextArea, num);
		try
		{
			ignoreSelectionChanges = true;
			SearchReplaceManager.ResetSearch();
			switch (action)
			{
			case SearchType.Find:
				SearchInFilesManager.FindAll(num, num2 - num, monitor);
				break;
			case SearchType.Replace:
				SearchReplaceManager.MarkAll(num, num2 - num, monitor);
				break;
			case SearchType.BookMark:
				SearchReplaceManager.ReplaceAll(num, num2 - num, monitor);
				break;
			}
			SearchReplaceUtilities.SelectText(textEditor, num, num2);
		}
		finally
		{
			ignoreSelectionChanges = false;
		}
	}

	public void Replace()
	{
		int num = Math.Min(selection.Offset, selection.EndOffset);
		int num2 = Math.Max(selection.Offset, selection.EndOffset);
		if (findFirst)
		{
			SetCaretPosition(textEditor.ActiveTextAreaControl.TextArea, num);
		}
		try
		{
			ignoreSelectionChanges = true;
			if (findFirst)
			{
				findFirst = false;
				SearchReplaceManager.ResetSearch();
				SearchReplaceManager.ReplaceFirstInSelection(num, num2 - num, null);
				return;
			}
			findFirst = !SearchReplaceManager.ReplaceNextInSelection(null);
			if (findFirst)
			{
				SearchReplaceUtilities.SelectText(textEditor, num, num2);
			}
		}
		finally
		{
			ignoreSelectionChanges = false;
		}
	}

	public void FindNext()
	{
		int num = Math.Min(selection.Offset, selection.EndOffset);
		int num2 = Math.Max(selection.Offset, selection.EndOffset);
		if (findFirst)
		{
			SetCaretPosition(textEditor.ActiveTextAreaControl.TextArea, num);
		}
		try
		{
			ignoreSelectionChanges = true;
			if (findFirst)
			{
				findFirst = false;
				SearchReplaceManager.ResetSearch();
				SearchReplaceManager.FindFirstInSelection(num, num2 - num, null);
				return;
			}
			findFirst = !SearchReplaceManager.FindNextInSelection(null);
			if (findFirst)
			{
				SearchReplaceUtilities.SelectText(textEditor, num, num2);
			}
		}
		finally
		{
			ignoreSelectionChanges = false;
		}
	}

	public void Init()
	{
		findFirst = true;
	}

	private void DoActiveChanged()
	{
		if (ActiveChanged != null)
		{
			ActiveChanged(this, EventArgs.Empty);
		}
	}

	private void TextSelectionChanged(object source, EventArgs e)
	{
		if (!ignoreSelectionChanges)
		{
			LoggingService.Debug("TextSelectionChanged.");
			selection = GetCurrentTextSelection();
			findFirst = true;
			DoActiveChanged();
		}
	}

	private void WorkbenchWindowChanged(object source, EventArgs e)
	{
		TextEditorControl activeTextEditor = SearchReplaceUtilities.GetActiveTextEditor();
		if (activeTextEditor != textEditor)
		{
			AddSelectionChangedHandler(activeTextEditor);
			TextSelectionChanged(source, e);
			DoActiveChanged();
		}
	}

	private static bool IsMultipleLineSelection(ISelection selection)
	{
		if (IsTextSelected(selection))
		{
			return selection.SelectedText.IndexOf('\n') != -1;
		}
		return false;
	}

	private static bool IsTextSelected(ISelection selection)
	{
		if (selection != null)
		{
			return !selection.IsEmpty;
		}
		return false;
	}

	private static ISelection GetCurrentTextSelection()
	{
		TextEditorControl activeTextEditor = SearchReplaceUtilities.GetActiveTextEditor();
		if (activeTextEditor != null)
		{
			SelectionManager selectionManager = activeTextEditor.ActiveTextAreaControl.SelectionManager;
			if (selectionManager.HasSomethingSelected)
			{
				return selectionManager.SelectionCollection[0];
			}
		}
		return null;
	}

	private void RemoveSelectionChangedHandler()
	{
		if (textEditor != null)
		{
			textEditor.ActiveTextAreaControl.SelectionManager.SelectionChanged -= TextSelectionChanged;
		}
	}

	private void AddSelectionChangedHandler(TextEditorControl textEditor)
	{
		RemoveSelectionChangedHandler();
		this.textEditor = textEditor;
		if (textEditor != null)
		{
			this.textEditor.ActiveTextAreaControl.SelectionManager.SelectionChanged += TextSelectionChanged;
		}
	}

	private void SetCaretPosition(TextArea textArea, int offset)
	{
		textArea.Caret.Position = textArea.Document.OffsetToPosition(offset);
	}
}
