using System;
using System.Collections.Generic;

namespace SearchAndReplace;

public class SearchAllFinishedEventArgs : EventArgs
{
	private string pattern;

	private List<SearchResult> results;

	public string Pattern
	{
		get
		{
			return pattern;
		}
		set
		{
			pattern = value;
		}
	}

	public List<SearchResult> Results
	{
		get
		{
			return results;
		}
		set
		{
			results = value;
		}
	}

	public SearchAllFinishedEventArgs(string pattern, List<SearchResult> results)
	{
		this.pattern = pattern;
		this.results = results;
	}
}
