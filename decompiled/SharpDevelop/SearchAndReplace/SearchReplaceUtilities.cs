using System;
using System.Collections;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public sealed class SearchReplaceUtilities
{
	private static ArrayList excludedFileExtensions;

	public static bool IsTextAreaSelected
	{
		get
		{
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
			{
				return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent is ITextEditorControlProvider;
			}
			return false;
		}
	}

	public static TextEditorControl GetActiveTextEditor()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ViewContent is ITextEditorControlProvider)
		{
			return ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
		}
		return null;
	}

	public static bool IsWholeWordAt(ITextBufferStrategy document, int offset, int length)
	{
		if (offset - 1 < 0 || char.IsWhiteSpace(document.GetCharAt(offset - 1)))
		{
			if (offset + length + 1 < document.Length)
			{
				return char.IsWhiteSpace(document.GetCharAt(offset + length));
			}
			return true;
		}
		return false;
	}

	public static ISearchStrategy CreateSearchStrategy(SearchStrategyType type)
	{
		return type switch
		{
			SearchStrategyType.Normal => new BruteForceSearchStrategy(), 
			SearchStrategyType.RegEx => new RegExSearchStrategy(), 
			SearchStrategyType.Wildcard => new WildcardSearchStrategy(), 
			_ => throw new NotImplementedException("CreateSearchStrategy for type " + type), 
		};
	}

	public static bool IsSearchable(string fileName)
	{
		if (fileName == null)
		{
			return false;
		}
		if (excludedFileExtensions == null)
		{
			excludedFileExtensions = AddInTree.BuildItems("/AddIns/DefaultTextEditor/Search/ExcludedFileExtensions", null, throwOnNotFound: false);
		}
		string extension = Path.GetExtension(fileName);
		if (extension != null)
		{
			foreach (string excludedFileExtension in excludedFileExtensions)
			{
				if (string.Compare(excludedFileExtension, extension, ignoreCase: true) == 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void SelectText(TextEditorControl textArea, int offset, int endOffset)
	{
		int textLength = textArea.ActiveTextAreaControl.Document.TextLength;
		if (textLength < endOffset)
		{
			endOffset = textLength - 1;
		}
		textArea.ActiveTextAreaControl.Caret.Position = textArea.Document.OffsetToPosition(endOffset);
		textArea.ActiveTextAreaControl.TextArea.SelectionManager.ClearSelection();
		textArea.ActiveTextAreaControl.TextArea.SelectionManager.SetSelection(new DefaultSelection(textArea.Document, textArea.Document.OffsetToPosition(offset), textArea.Document.OffsetToPosition(endOffset)));
		textArea.Refresh();
	}

	public static bool TryFindFirst(string serachText, out int line, out int column)
	{
		line = 0;
		column = 0;
		SearchOptions.Preserve();
		SearchOptions.FindPattern = serachText;
		SearchOptions.MatchWholeWord = true;
		SearchOptions.MatchCase = false;
		SearchOptions.SearchStrategyType = SearchStrategyType.Normal;
		SearchOptions.LookIn = "";
		SearchOptions.LookInFiletypes = "";
		SearchOptions.ReplacePattern = "";
		SearchOptions.IncludeSubdirectories = true;
		SearchOptions.SearchAndReplaceBinding = SearchOptions.CurrentDocumentBinding;
		using (ProgressNotificationTaskInstance monitor = new ProgressNotificationTaskInstance("Searching: " + serachText))
		{
			Search search = new Search();
			search.TextIteratorBuilder = new ForwardTextIteratorBuilder();
			search.SearchStrategy = CreateSearchStrategy(SearchOptions.SearchStrategyType);
			search.DocumentIterator = SearchOptions.SearchAndReplaceBinding.GetIterator();
			search.Reset();
			if (!search.SearchStrategy.CompilePattern(monitor))
			{
				return false;
			}
			SearchResult searchResult = search.FindNext(monitor);
			if (searchResult == null)
			{
				return false;
			}
			searchResult.CreateDocument();
			IDocument document = searchResult.CreateDocument();
			if (document == null)
			{
				return false;
			}
			TextLocation startPosition = searchResult.GetStartPosition(document);
			line = startPosition.Line;
			column = startPosition.Column;
		}
		SearchOptions.Restore();
		return true;
	}
}
