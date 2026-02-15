using System;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class SearchReplaceManager
{
	public static SearchAndReplaceDialog SearchAndReplaceDialog;

	private static Search find;

	private static TextSelection textSelection;

	private static SearchResult lastResult;

	private static bool foundAtLeastOneItem;

	static SearchReplaceManager()
	{
		SearchAndReplaceDialog = null;
		find = new Search();
		lastResult = null;
		foundAtLeastOneItem = false;
		find.TextIteratorBuilder = new ForwardTextIteratorBuilder();
	}

	private static void SetSearchOptions()
	{
		find.SearchStrategy = SearchReplaceUtilities.CreateSearchStrategy(SearchOptions.SearchStrategyType);
		if (find.DocumentIterator == null)
		{
			find.DocumentIterator = SearchOptions.SearchAndReplaceBinding.GetIterator();
		}
	}

	public static void ResetSearch()
	{
		find.DocumentIterator = null;
	}

	public static void Replace(IProgressNotificationTaskInstance monitor)
	{
		SetSearchOptions();
		if (lastResult != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent is ITextEditorControlProvider { TextEditorControl: var textEditorControl })
		{
			SelectionManager selectionManager = textEditorControl.ActiveTextAreaControl.TextArea.SelectionManager;
			if (selectionManager.SelectionCollection.Count == 1 && selectionManager.SelectionCollection[0].Offset == lastResult.Offset && selectionManager.SelectionCollection[0].Length == lastResult.Length && lastResult.FileName == WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName)
			{
				string text = lastResult.TransformReplacePattern(SearchOptions.ReplacePattern);
				textEditorControl.BeginUpdate();
				selectionManager.ClearSelection();
				textEditorControl.Document.Replace(lastResult.Offset, lastResult.Length, text);
				textEditorControl.ActiveTextAreaControl.Caret.Position = textEditorControl.Document.OffsetToPosition(lastResult.Offset + text.Length);
				textEditorControl.EndUpdate();
			}
		}
		FindNext(monitor);
	}

	public static void ReplaceFirstInSelection(int offset, int length, IProgressNotificationTaskInstance monitor)
	{
		SetSearchOptions();
		FindFirstInSelection(offset, length, monitor);
	}

	public static bool ReplaceNextInSelection(IProgressNotificationTaskInstance monitor)
	{
		if (lastResult != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent is ITextEditorControlProvider { TextEditorControl: var textEditorControl })
		{
			SelectionManager selectionManager = textEditorControl.ActiveTextAreaControl.TextArea.SelectionManager;
			if (selectionManager.SelectionCollection.Count == 1 && selectionManager.SelectionCollection[0].Offset == lastResult.Offset && selectionManager.SelectionCollection[0].Length == lastResult.Length && lastResult.FileName == textEditorControl.FileName)
			{
				string text = lastResult.TransformReplacePattern(SearchOptions.ReplacePattern);
				textEditorControl.BeginUpdate();
				selectionManager.ClearSelection();
				textEditorControl.Document.Replace(lastResult.Offset, lastResult.Length, text);
				textEditorControl.ActiveTextAreaControl.Caret.Position = textEditorControl.Document.OffsetToPosition(lastResult.Offset + text.Length);
				textEditorControl.EndUpdate();
				textSelection.Length -= lastResult.Length - text.Length;
			}
		}
		return FindNextInSelection(monitor);
	}

	public static void MarkAll(IProgressNotificationTaskInstance monitor)
	{
		SetSearchOptions();
		ClearSelection();
		find.Reset();
		if (!find.SearchStrategy.CompilePattern(monitor))
		{
			return;
		}
		List<TextEditorControl> list = new List<TextEditorControl>();
		int num = 0;
		while (true)
		{
			SearchResult searchResult = find.FindNext(monitor);
			if (searchResult == null)
			{
				break;
			}
			MarkResult(list, searchResult);
			num++;
		}
		find.Reset();
		foreach (TextEditorControl item in list)
		{
			item.Refresh();
		}
		ShowMarkDoneMessage(num, monitor);
	}

	public static void MarkAll(int offset, int length, IProgressNotificationTaskInstance monitor)
	{
		SetSearchOptions();
		find.Reset();
		if (!find.SearchStrategy.CompilePattern(monitor))
		{
			return;
		}
		List<TextEditorControl> list = new List<TextEditorControl>();
		int num = 0;
		while (true)
		{
			SearchResult searchResult = find.FindNext(offset, length);
			if (searchResult == null)
			{
				break;
			}
			MarkResult(list, searchResult);
			num++;
		}
		find.Reset();
		foreach (TextEditorControl item in list)
		{
			item.Refresh();
		}
		ShowMarkDoneMessage(num, monitor);
	}

	private static void MarkResult(List<TextEditorControl> textAreas, SearchResult result)
	{
		TextEditorControl textEditorControl = OpenTextArea(result.FileName);
		if (textEditorControl != null)
		{
			if (!textAreas.Contains(textEditorControl))
			{
				textAreas.Add(textEditorControl);
			}
			textEditorControl.ActiveTextAreaControl.Caret.Position = textEditorControl.Document.OffsetToPosition(result.Offset);
			int lineNumberForOffset = textEditorControl.Document.GetLineNumberForOffset(result.Offset);
			textEditorControl.Document.BookmarkManager.SetMarkAt(lineNumberForOffset);
		}
	}

	private static void ShowMarkDoneMessage(int count, IProgressNotificationTaskInstance monitor)
	{
		if (count == 0)
		{
			ShowNotFoundMessage(monitor);
		}
		else
		{
			MessageService.ShowMessage("${res:ICSharpCode.TextEditor.Document.SearchReplaceManager.MarkAllDone}", "${res:Global.FinishedCaptionText}");
		}
	}

	private static void ShowReplaceDoneMessage(int count, IProgressNotificationTaskInstance monitor)
	{
		if (count == 0)
		{
			ShowNotFoundMessage(monitor);
		}
		else
		{
			MessageService.ShowMessage(count + " ${res:ICSharpCode.TextEditor.Document.SearchReplaceManager.ReplaceAllDone}", "Clarion");
		}
	}

	public static void ReplaceAll(IProgressNotificationTaskInstance monitor)
	{
		SetSearchOptions();
		ClearSelection();
		find.Reset();
		if (!find.SearchStrategy.CompilePattern(monitor))
		{
			return;
		}
		List<TextEditorControl> list = new List<TextEditorControl>();
		TextEditorControl textEditorControl = null;
		int num = 0;
		while (true)
		{
			SearchResult searchResult = find.FindNext(monitor);
			if (searchResult == null)
			{
				break;
			}
			if (textEditorControl == null || textEditorControl.FileName != searchResult.FileName)
			{
				textEditorControl = OpenTextArea(searchResult.FileName);
				if (textEditorControl != null && !list.Contains(textEditorControl))
				{
					textEditorControl.BeginUpdate();
					textEditorControl.ActiveTextAreaControl.TextArea.SelectionManager.SelectionCollection.Clear();
					list.Add(textEditorControl);
				}
			}
			if (textEditorControl != null)
			{
				string text = searchResult.TransformReplacePattern(SearchOptions.ReplacePattern);
				find.Replace(searchResult.Offset, searchResult.Length, text);
				if (find.CurrentDocumentInformation.Document == null)
				{
					textEditorControl.Document.Replace(searchResult.Offset, searchResult.Length, text);
				}
			}
			else
			{
				num--;
			}
			num++;
		}
		if (num != 0)
		{
			foreach (TextEditorControl item in list)
			{
				item.EndUpdate();
				item.Refresh();
			}
		}
		ShowReplaceDoneMessage(num, monitor);
		find.Reset();
	}

	public static void ReplaceAll(int offset, int length, IProgressNotificationTaskInstance monitor)
	{
		SetSearchOptions();
		find.Reset();
		if (!find.SearchStrategy.CompilePattern(monitor))
		{
			return;
		}
		int num = 0;
		while (true)
		{
			SearchResult searchResult = find.FindNext(offset, length);
			if (searchResult == null)
			{
				break;
			}
			string text = searchResult.TransformReplacePattern(SearchOptions.ReplacePattern);
			find.Replace(searchResult.Offset, searchResult.Length, text);
			length -= searchResult.Length - text.Length;
			find.CurrentDocumentInformation.CurrentOffset = searchResult.Offset + text.Length - 1;
			num++;
		}
		ShowReplaceDoneMessage(num, monitor);
	}

	public static void FindNext(IProgressNotificationTaskInstance monitor)
	{
		SetSearchOptions();
		if (find == null || SearchOptions.FindPattern == null || SearchOptions.FindPattern.Length == 0)
		{
			return;
		}
		if (!find.SearchStrategy.CompilePattern(monitor))
		{
			find.Reset();
			lastResult = null;
			return;
		}
		if (find.DocumentIterator is DummyDocumentIterator && ((DummyDocumentIterator)find.DocumentIterator).InvalidDirectory)
		{
			((DummyDocumentIterator)find.DocumentIterator).InvalidDirectoryMessage();
			find.DocumentIterator = null;
			return;
		}
		TextEditorControl textEditorControl = null;
		while (textEditorControl == null)
		{
			SearchResult searchResult = find.FindNext();
			if (searchResult == null)
			{
				if (find.TextIterator == null || find.TextIterator.Position + 1 < find.TextIterator.TextBuffer.Length)
				{
					ShowNotFoundMessage(monitor);
				}
				find.Reset();
				lastResult = null;
				break;
			}
			textEditorControl = OpenTextArea(searchResult.FileName);
			if (textEditorControl != null)
			{
				if (lastResult != null && lastResult.FileName == searchResult.FileName && textEditorControl.ActiveTextAreaControl.Caret.Offset != lastResult.Offset + lastResult.Length)
				{
					find.Reset();
				}
				int num = Math.Min(textEditorControl.Document.TextLength, Math.Max(0, searchResult.Offset));
				int endOffset = Math.Min(textEditorControl.Document.TextLength, num + searchResult.Length);
				SearchReplaceUtilities.SelectText(textEditorControl, num, endOffset);
				lastResult = searchResult;
			}
		}
	}

	public static void FindFirstInSelection(int offset, int length, IProgressNotificationTaskInstance monitor)
	{
		foundAtLeastOneItem = false;
		textSelection = null;
		SetSearchOptions();
		if (find != null && SearchOptions.FindPattern != null && SearchOptions.FindPattern.Length != 0)
		{
			if (!find.SearchStrategy.CompilePattern(monitor))
			{
				find.Reset();
				lastResult = null;
			}
			else
			{
				textSelection = new TextSelection(offset, length);
				FindNextInSelection(monitor);
			}
		}
	}

	public static bool FindNextInSelection(IProgressNotificationTaskInstance monitor)
	{
		TextEditorControl textEditorControl = null;
		while (textEditorControl == null)
		{
			SearchResult searchResult = find.FindNext(textSelection.Offset, textSelection.Length);
			if (searchResult == null)
			{
				if (!foundAtLeastOneItem)
				{
					ShowNotFoundMessage(monitor);
				}
				find.Reset();
				lastResult = null;
				foundAtLeastOneItem = false;
				return false;
			}
			textEditorControl = OpenTextArea(searchResult.FileName);
			if (textEditorControl != null)
			{
				foundAtLeastOneItem = true;
				if (lastResult != null && lastResult.FileName == searchResult.FileName)
				{
					_ = textEditorControl.ActiveTextAreaControl.Caret.Offset;
					_ = lastResult.Offset + lastResult.Length;
				}
				int num = Math.Min(textEditorControl.Document.TextLength, Math.Max(0, searchResult.Offset));
				int endOffset = Math.Min(textEditorControl.Document.TextLength, num + searchResult.Length);
				SearchReplaceUtilities.SelectText(textEditorControl, num, endOffset);
				lastResult = searchResult;
			}
		}
		return true;
	}

	private static void ShowNotFoundMessage(IProgressNotificationTaskInstance monitor)
	{
		MessageService.ShowMessage(ResourceService.GetString("Dialog.NewProject.SearchReplace.SearchStringNotFound"), ResourceService.GetString("Dialog.NewProject.SearchReplace.SearchStringNotFound.Title"));
	}

	private static TextEditorControl OpenTextArea(string fileName)
	{
		ITextEditorControlProvider textEditorControlProvider = null;
		if (fileName != null)
		{
			IViewContent openFileViewContent = FileService.GetOpenFileViewContent(fileName);
			if (openFileViewContent != null)
			{
				textEditorControlProvider = openFileViewContent as ITextEditorControlProvider;
			}
		}
		else
		{
			textEditorControlProvider = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent as ITextEditorControlProvider;
		}
		return textEditorControlProvider?.TextEditorControl;
	}

	private static void ClearSelection()
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent is ITextEditorControlProvider textEditorControlProvider)
		{
			textEditorControlProvider.TextEditorControl.ActiveTextAreaControl.TextArea.SelectionManager.ClearSelection();
		}
	}
}
