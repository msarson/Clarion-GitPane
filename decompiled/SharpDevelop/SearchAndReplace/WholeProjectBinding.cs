using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace SearchAndReplace;

public class WholeProjectBinding : AbstractSearchAndReplaceBinding
{
	public override bool Active => ProjectService.CurrentProject != null;

	public override bool HasFullSearcher => false;

	public override bool NeedsFilePattern => true;

	public override bool NeedsSubFolders => false;

	public override bool NeedsFileList => false;

	public WholeProjectBinding()
	{
		ProjectService.CurrentProjectChanged += CurrentProjectChanged;
		ProjectService.SolutionClosed += SolutionClosed;
	}

	private void CurrentProjectChanged(object sender, ProjectEventArgs e)
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
		return new WholeProjectDocumentIterator(SearchOptions.LookInFiletypes);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			ProjectService.SolutionClosed -= SolutionClosed;
			ProjectService.CurrentProjectChanged -= CurrentProjectChanged;
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public override string ToString()
	{
		return ResourceService.GetString("Dialog.NewProject.SearchReplace.LookIn.WholeProject");
	}
}
