using ICSharpCode.Core;
using SearchAndReplace;

namespace SoftVelocity.Common;

public class RedirectionSearchBinding : AbstractSearchAndReplaceBinding
{
	public override bool Active => true;

	public override bool HasFullSearcher => false;

	public override bool NeedsFilePattern => true;

	public override bool NeedsSubFolders => false;

	public override bool NeedsFileList => false;

	public override ISearcher GetSearcher()
	{
		return null;
	}

	public override IDocumentIterator GetIterator()
	{
		return (IDocumentIterator)(object)new RedirectionDocumentIterator(SearchOptions.LookInFiletypes);
	}

	public override string ToString()
	{
		return ResourceService.GetString("SoftVelocity.SearchReplace.LookIn.Redirection");
	}
}
