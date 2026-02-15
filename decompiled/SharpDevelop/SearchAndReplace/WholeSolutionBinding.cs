using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace SearchAndReplace;

public class WholeSolutionBinding : AbstractSearchAndReplaceBinding
{
	public override bool Active => ProjectService.OpenSolution != null;

	public override bool HasFullSearcher => false;

	public override bool NeedsFilePattern => true;

	public override bool NeedsSubFolders => false;

	public override bool NeedsFileList => false;

	public WholeSolutionBinding()
	{
		ProjectService.SolutionClosed += SolutionClosed;
		ProjectService.SolutionLoaded += SolutionLoaded;
	}

	private void SolutionLoaded(object sender, SolutionEventArgs e)
	{
		DoActiveChanged();
	}

	private void SolutionClosed(object sender, EventArgs e)
	{
		DoActiveChanged();
	}

	public override ISearcher GetSearcher()
	{
		return null;
	}

	public override IDocumentIterator GetIterator()
	{
		return new WholeSolutionDocumentIterator(SearchOptions.LookInFiletypes);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			ProjectService.SolutionClosed -= SolutionClosed;
			ProjectService.SolutionLoaded -= SolutionLoaded;
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public override string ToString()
	{
		return ResourceService.GetString("Dialog.NewProject.SearchReplace.LookIn.WholeSolution");
	}
}
