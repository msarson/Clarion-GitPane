using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class CurrentDocumentDirectoryBinding : AbstractSearchAndReplaceBinding
{
	public override bool Active
	{
		get
		{
			try
			{
				return Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName)));
			}
			catch
			{
				return false;
			}
		}
	}

	public override bool HasFullSearcher => false;

	public override bool NeedsFilePattern => true;

	public override bool NeedsSubFolders => true;

	public override bool NeedsFileList => false;

	public CurrentDocumentDirectoryBinding()
	{
		SearchOptions.CurrentDocumentDirectoryBinding = this;
	}

	public override ISearcher GetSearcher()
	{
		return null;
	}

	public override IDocumentIterator GetIterator()
	{
		return new InCurrentDirectory(SearchOptions.LookInFiletypes, SearchOptions.IncludeSubdirectories);
	}

	public override string ToString()
	{
		return ResourceService.GetString("Dialog.NewProject.SearchReplace.LookIn.CurrentDocumentDirectory");
	}
}
