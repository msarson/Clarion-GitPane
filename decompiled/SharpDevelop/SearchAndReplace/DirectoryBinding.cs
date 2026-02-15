using System;
using System.IO;
using ICSharpCode.Core;

namespace SearchAndReplace;

public class DirectoryBinding : AbstractSearchAndReplaceBinding
{
	public override bool Active => true;

	public override bool HasFullSearcher => false;

	public override bool NeedsFilePattern => true;

	public override bool NeedsSubFolders => true;

	public override bool NeedsFileList => true;

	public DirectoryBinding()
	{
		SearchOptions.DirectoryBinding = this;
	}

	public override ISearcher GetSearcher()
	{
		return null;
	}

	public override IDocumentIterator GetIterator()
	{
		try
		{
			if (!Directory.Exists(SearchOptions.LookIn))
			{
				return new DummyDocumentIterator(invalidDirectory: true);
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowMessage(ex.Message);
			return new DummyDocumentIterator();
		}
		return new DirectoryDocumentIterator(SearchOptions.LookIn, SearchOptions.LookInFiletypes, SearchOptions.IncludeSubdirectories);
	}

	public override string ToString()
	{
		return SearchOptions.LookIn;
	}
}
