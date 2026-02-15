using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class CurrentDocumentBinding : AbstractSearchAndReplaceBinding
{
	public override bool Active => SearchReplaceUtilities.IsTextAreaSelected;

	public override bool HasFullSearcher => false;

	public override bool NeedsFilePattern => false;

	public override bool NeedsSubFolders => false;

	public override bool NeedsFileList => false;

	public CurrentDocumentBinding()
	{
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += ActiveWorkbenchWindowChanged;
		SearchOptions.CurrentDocumentBinding = this;
	}

	private void ActiveWorkbenchWindowChanged(object sender, EventArgs e)
	{
		DoActiveChanged();
	}

	public override ISearcher GetSearcher()
	{
		return null;
	}

	public override IDocumentIterator GetIterator()
	{
		return new CurrentDocumentIterator();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged -= ActiveWorkbenchWindowChanged;
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public override string ToString()
	{
		return ResourceService.GetString("Dialog.NewProject.SearchReplace.LookIn.CurrentDocument");
	}
}
