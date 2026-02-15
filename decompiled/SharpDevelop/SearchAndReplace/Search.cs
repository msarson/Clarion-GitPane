using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class Search
{
	private ISearchStrategy searchStrategy;

	private IDocumentIterator documentIterator;

	private ITextIterator textIterator;

	private ITextIteratorBuilder textIteratorBuilder;

	private ProvidedDocumentInformation info;

	public ProvidedDocumentInformation CurrentDocumentInformation => info;

	public ITextIteratorBuilder TextIteratorBuilder
	{
		get
		{
			return textIteratorBuilder;
		}
		set
		{
			textIteratorBuilder = value;
		}
	}

	public ITextIterator TextIterator => textIterator;

	public ISearchStrategy SearchStrategy
	{
		get
		{
			return searchStrategy;
		}
		set
		{
			searchStrategy = value;
		}
	}

	public IDocumentIterator DocumentIterator
	{
		get
		{
			return documentIterator;
		}
		set
		{
			documentIterator = value;
		}
	}

	private SearchResult CreateNamedSearchResult(SearchResult pos)
	{
		if (info == null || pos == null)
		{
			return null;
		}
		pos.ProvidedDocumentInformation = info;
		return pos;
	}

	public void Reset()
	{
		documentIterator.Reset();
		textIterator = null;
	}

	public void Replace(int offset, int length, string pattern)
	{
		if (CurrentDocumentInformation != null && TextIterator != null)
		{
			CurrentDocumentInformation.Replace(offset, length, pattern);
			TextIterator.InformReplace(offset, length, pattern.Length);
		}
	}

	public SearchResult FindNext()
	{
		return FindNext(null);
	}

	public SearchResult FindNext(IProgressNotificationTaskInstance monitor)
	{
		if (monitor != null && monitor.IsCancelled)
		{
			return null;
		}
		if (documentIterator is DirectoryDocumentIterator)
		{
			((DirectoryDocumentIterator)documentIterator).ProgressMonitor = monitor;
		}
		if (info != null && textIterator != null && documentIterator.CurrentFileName != null)
		{
			ProvidedDocumentInformation current = documentIterator.Current;
			if (!info.Equals(current))
			{
				info = current;
				textIterator = textIteratorBuilder.BuildTextIterator(info);
				textIterator.ResetCaret();
			}
			else
			{
				if (monitor == null && info.CurrentOffset < textIterator.Position)
				{
					textIterator.ResetCaret();
				}
				textIterator.Position = info.CurrentOffset;
			}
			SearchResult searchResult = CreateNamedSearchResult(searchStrategy.FindNext(textIterator));
			if (searchResult != null)
			{
				info.CurrentOffset = textIterator.Position;
				return searchResult;
			}
		}
		if (documentIterator.MoveForward())
		{
			info = documentIterator.Current;
			if (info != null && info.TextBuffer != null && info.EndOffset >= 0 && info.EndOffset <= info.TextBuffer.Length)
			{
				textIterator = textIteratorBuilder.BuildTextIterator(info);
				if (monitor != null)
				{
					monitor.TaskText = "Searching " + info.FileName;
				}
			}
			else
			{
				textIterator = null;
			}
			return FindNext(monitor);
		}
		return null;
	}

	public SearchResult FindNext(int offset, int length)
	{
		if (info != null && textIterator != null && documentIterator.CurrentFileName != null)
		{
			ProvidedDocumentInformation current = documentIterator.Current;
			if (!info.Equals(current))
			{
				info = current;
				textIterator = textIteratorBuilder.BuildTextIterator(info);
			}
			else
			{
				textIterator.Position = info.CurrentOffset;
			}
			SearchResult searchResult = CreateNamedSearchResult(searchStrategy.FindNext(textIterator, offset, length));
			if (searchResult != null)
			{
				info.CurrentOffset = textIterator.Position;
				return searchResult;
			}
		}
		if (documentIterator.MoveForward())
		{
			info = documentIterator.Current;
			if (info != null && info.TextBuffer != null && info.EndOffset >= 0 && info.EndOffset < info.TextBuffer.Length)
			{
				textIterator = textIteratorBuilder.BuildTextIterator(info);
			}
			else
			{
				textIterator = null;
			}
			return FindNext(offset, length);
		}
		return null;
	}
}
