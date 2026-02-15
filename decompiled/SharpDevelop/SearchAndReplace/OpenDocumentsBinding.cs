using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class OpenDocumentsBinding : AbstractSearchAndReplaceBinding
{
	public override bool Active
	{
		get
		{
			foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				if (item is ITextEditorControlProvider)
				{
					return true;
				}
			}
			return false;
		}
	}

	public override bool HasFullSearcher => false;

	public override bool NeedsFilePattern => false;

	public override bool NeedsSubFolders => false;

	public override bool NeedsFileList => false;

	public OpenDocumentsBinding()
	{
		WorkbenchSingleton.Workbench.ViewClosed += ViewOpenedClosed;
		WorkbenchSingleton.Workbench.ViewOpened += ViewOpenedClosed;
	}

	private void ViewOpenedClosed(object sender, ViewContentEventArgs e)
	{
		DoActiveChanged();
	}

	public override ISearcher GetSearcher()
	{
		return null;
	}

	public override IDocumentIterator GetIterator()
	{
		return new AllOpenDocumentIterator();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			WorkbenchSingleton.Workbench.ViewClosed -= ViewOpenedClosed;
			WorkbenchSingleton.Workbench.ViewOpened -= ViewOpenedClosed;
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public override string ToString()
	{
		return ResourceService.GetString("Dialog.NewProject.SearchReplace.LookIn.AllOpenDocuments");
	}
}
