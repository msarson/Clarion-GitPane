using System;

namespace SearchAndReplace;

public abstract class AbstractSearchAndReplaceBinding : IDisposable
{
	public EventHandler ActiveChanged;

	public abstract bool Active { get; }

	public abstract bool HasFullSearcher { get; }

	public abstract bool NeedsFilePattern { get; }

	public abstract bool NeedsSubFolders { get; }

	public abstract bool NeedsFileList { get; }

	public abstract ISearcher GetSearcher();

	public abstract IDocumentIterator GetIterator();

	protected void DoActiveChanged()
	{
		if (ActiveChanged != null)
		{
			ActiveChanged(this, EventArgs.Empty);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	~AbstractSearchAndReplaceBinding()
	{
		Dispose(disposing: false);
	}
}
