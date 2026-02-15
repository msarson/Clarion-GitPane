using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public static class SearchInFilesManager
{
	private static Search find;

	private static string currentFileName;

	private static List<SearchAllFinishedEventArgs> lastSearches;

	public static List<SearchAllFinishedEventArgs> LastSearches => lastSearches;

	public static event SearchAllFinishedEventHandler SearchAllFinished;

	static SearchInFilesManager()
	{
		find = new Search();
		currentFileName = string.Empty;
		lastSearches = new List<SearchAllFinishedEventArgs>();
		find.TextIteratorBuilder = new ForwardTextIteratorBuilder();
	}

	private static void SetSearchOptions()
	{
		find.SearchStrategy = SearchReplaceUtilities.CreateSearchStrategy(SearchOptions.SearchStrategyType);
		find.DocumentIterator = SearchOptions.SearchAndReplaceBinding.GetIterator();
	}

	private static bool InitializeSearchInFiles(IProgressNotificationTaskInstance monitor)
	{
		SetSearchOptions();
		find.Reset();
		if (!find.SearchStrategy.CompilePattern(monitor))
		{
			return false;
		}
		currentFileName = string.Empty;
		return true;
	}

	private static void FinishSearchInFiles(List<SearchResult> results)
	{
		ShowSearchResults(SearchOptions.FindPattern, results);
	}

	public static void GoToFirstResultIfUnique()
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(GoToFirstResultIfUnique);
			return;
		}
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(SearchResultPanel));
		if (pad != null)
		{
			if (SearchResultPanel.Instance.ResultsCount == 1)
			{
				SearchResultPanel.Instance.GoToFirstResultIfUnique();
			}
			else if (SearchResultPanel.Instance.ResultsCount > 1)
			{
				pad.BringPadToFront();
			}
		}
	}

	public static void ShowSearchResults(string pattern, List<SearchResult> results)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(ShowSearchResults, pattern, results);
			return;
		}
		SearchAllFinishedEventArgs e = new SearchAllFinishedEventArgs(pattern, results);
		OnSearchAllFinished(e);
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(SearchResultPanel));
		if (pad != null)
		{
			if (SearchOptions.ShowResults)
			{
				pad.BringPadToFront();
			}
			else
			{
				pad.ShowPad();
			}
			SearchResultPanel.Instance.ShowSearchResults(pattern, results);
		}
		else
		{
			MessageService.ShowError("SearchResultPanel can't be created.");
		}
	}

	public static void FindAll(IProgressNotificationTaskInstance monitor)
	{
		if (!InitializeSearchInFiles(monitor))
		{
			return;
		}
		List<SearchResult> list = new List<SearchResult>();
		while (!monitor.IsCancelled)
		{
			SearchResult searchResult = find.FindNext(monitor);
			if (searchResult == null)
			{
				break;
			}
			list.Add(searchResult);
		}
		FinishSearchInFiles(list);
	}

	public static void FindAll(int offset, int length, IProgressNotificationTaskInstance monitor)
	{
		if (!InitializeSearchInFiles(monitor))
		{
			return;
		}
		List<SearchResult> list = new List<SearchResult>();
		if (monitor != null)
		{
			while (!monitor.IsCancelled)
			{
				SearchResult searchResult = find.FindNext(offset, length);
				if (searchResult == null)
				{
					break;
				}
				list.Add(searchResult);
			}
		}
		FinishSearchInFiles(list);
	}

	private static void OnSearchAllFinished(SearchAllFinishedEventArgs e)
	{
		lastSearches.Insert(0, e);
		if (SearchInFilesManager.SearchAllFinished != null)
		{
			SearchInFilesManager.SearchAllFinished(null, e);
		}
	}
}
