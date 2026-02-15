using System;
using ICSharpCode.Core;

namespace SearchAndReplace;

public class CurrentSelectionBinding : AbstractSearchAndReplaceBinding
{
	private CurrentSelectionSearcher searcher;

	private CurrentSelectionSearcher Searcher
	{
		get
		{
			if (searcher == null)
			{
				searcher = new CurrentSelectionSearcher();
				CurrentSelectionSearcher currentSelectionSearcher = searcher;
				currentSelectionSearcher.ActiveChanged = (EventHandler)Delegate.Combine(currentSelectionSearcher.ActiveChanged, new EventHandler(SearcherActiveChanged));
			}
			return searcher;
		}
	}

	public override bool Active => Searcher.Active;

	public override bool HasFullSearcher => true;

	public override bool NeedsFilePattern => false;

	public override bool NeedsSubFolders => false;

	public override bool NeedsFileList => false;

	public CurrentSelectionBinding()
	{
		SearchOptions.CurrentSelectionBinding = this;
	}

	private void SearcherActiveChanged(object source, EventArgs e)
	{
		DoActiveChanged();
	}

	public override ISearcher GetSearcher()
	{
		return Searcher;
	}

	public override IDocumentIterator GetIterator()
	{
		return new CurrentDocumentIterator();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (searcher != null)
			{
				CurrentSelectionSearcher currentSelectionSearcher = searcher;
				currentSelectionSearcher.ActiveChanged = (EventHandler)Delegate.Combine(currentSelectionSearcher.ActiveChanged, ActiveChanged);
				searcher.Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public override string ToString()
	{
		return ResourceService.GetString("Dialog.NewProject.SearchReplace.LookIn.CurrentSelection");
	}
}
