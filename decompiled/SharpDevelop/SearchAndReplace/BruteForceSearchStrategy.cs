using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class BruteForceSearchStrategy : ISearchStrategy
{
	private string searchPattern;

	private bool MatchCaseSensitive(ITextBufferStrategy document, int offset, string pattern)
	{
		for (int i = 0; i < pattern.Length; i++)
		{
			if (offset + i >= document.Length || document.GetCharAt(offset + i) != pattern[i])
			{
				return false;
			}
		}
		return true;
	}

	private bool MatchCaseInsensitive(ITextBufferStrategy document, int offset, string pattern)
	{
		for (int i = 0; i < pattern.Length; i++)
		{
			if (offset + i >= document.Length || char.ToUpper(document.GetCharAt(offset + i)) != pattern[i])
			{
				return false;
			}
		}
		return true;
	}

	private bool IsWholeWordAt(ITextBufferStrategy document, int offset, int length)
	{
		if (offset - 1 < 0 || !IsLetterOrDigit(document.GetCharAt(offset - 1)))
		{
			if (offset + length + 1 < document.Length)
			{
				return !IsLetterOrDigit(document.GetCharAt(offset + length));
			}
			return true;
		}
		return false;
	}

	private bool IsLetterOrDigit(char c)
	{
		if (!char.IsLetterOrDigit(c))
		{
			return c == '_';
		}
		return true;
	}

	private bool IsInReadOnlyLine(ITextIterator textIterator)
	{
		IDocument document = textIterator.Document;
		if (document != null)
		{
			ICustomLineManager customLineManager = document.CustomLineManager;
			if (customLineManager != null)
			{
				int lineNumberForOffset = textIterator.Document.GetLineNumberForOffset(textIterator.Position);
				return customLineManager.IsReadOnly(lineNumberForOffset, defaultReadOnly: false);
			}
		}
		return false;
	}

	private int InternalFindNext(ITextIterator textIterator)
	{
		while (textIterator.MoveAhead(1))
		{
			if ((SearchOptions.MatchCase ? MatchCaseSensitive(textIterator.TextBuffer, textIterator.Position, searchPattern) : MatchCaseInsensitive(textIterator.TextBuffer, textIterator.Position, searchPattern)) && (!SearchOptions.MatchWholeWord || IsWholeWordAt(textIterator.TextBuffer, textIterator.Position, searchPattern.Length)) && (SearchOptions.IncludeReadOnlyBlocks || (!SearchOptions.IncludeReadOnlyBlocks && !IsInReadOnlyLine(textIterator))))
			{
				return textIterator.Position;
			}
		}
		return -1;
	}

	private int InternalFindNext(ITextIterator textIterator, int offset, int length)
	{
		while (textIterator.MoveAhead(1) && TextSelection.IsInsideRange(textIterator.Position, offset, length))
		{
			if ((SearchOptions.MatchCase ? MatchCaseSensitive(textIterator.TextBuffer, textIterator.Position, searchPattern) : MatchCaseInsensitive(textIterator.TextBuffer, textIterator.Position, searchPattern)) && (!SearchOptions.MatchWholeWord || IsWholeWordAt(textIterator.TextBuffer, textIterator.Position, searchPattern.Length)))
			{
				if (!TextSelection.IsInsideRange(textIterator.Position + searchPattern.Length - 1, offset, length))
				{
					return -1;
				}
				if (SearchOptions.IncludeReadOnlyBlocks || (!SearchOptions.IncludeReadOnlyBlocks && !IsInReadOnlyLine(textIterator)))
				{
					return textIterator.Position;
				}
			}
		}
		return -1;
	}

	public bool CompilePattern(IProgressNotificationTaskInstance monitor)
	{
		searchPattern = (SearchOptions.MatchCase ? SearchOptions.FindPattern : SearchOptions.FindPattern.ToUpper());
		return true;
	}

	public SearchResult FindNext(ITextIterator textIterator)
	{
		int offset = InternalFindNext(textIterator);
		return GetSearchResult(offset);
	}

	public SearchResult FindNext(ITextIterator textIterator, int offset, int length)
	{
		int offset2 = InternalFindNext(textIterator, offset, length);
		return GetSearchResult(offset2);
	}

	private SearchResult GetSearchResult(int offset)
	{
		if (offset < 0)
		{
			return null;
		}
		return new SearchResult(offset, searchPattern.Length);
	}
}
