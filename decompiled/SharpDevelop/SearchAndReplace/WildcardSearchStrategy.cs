using System.Collections;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class WildcardSearchStrategy : ISearchStrategy
{
	private enum CommandType
	{
		Match,
		AnyZeroOrMore,
		AnySingle,
		AnyDigit,
		AnyInList,
		NoneInList
	}

	private class Command
	{
		public CommandType CommandType;

		public char SingleChar;

		public string CharList = string.Empty;
	}

	private ArrayList patternProgram;

	private int curMatchEndOffset = -1;

	private void CompilePattern(string pattern, bool ignoreCase)
	{
		patternProgram = new ArrayList();
		Command command;
		for (int i = 0; i < pattern.Length; patternProgram.Add(command), i++)
		{
			command = new Command();
			switch (pattern[i])
			{
			case '#':
				command.CommandType = CommandType.AnyDigit;
				continue;
			case '*':
				command.CommandType = CommandType.AnyZeroOrMore;
				continue;
			case '?':
				command.CommandType = CommandType.AnySingle;
				continue;
			case '[':
			{
				int num = pattern.IndexOf(']', i);
				if (num > 0)
				{
					command.CommandType = CommandType.AnyInList;
					string text = pattern.Substring(i + 1, num - i - 1);
					if (text[0] == '!')
					{
						command.CommandType = CommandType.NoneInList;
						text = text.Substring(1);
					}
					command.CharList = (ignoreCase ? text.ToUpper() : text);
					i = num;
					continue;
				}
				break;
			}
			}
			command.CommandType = CommandType.Match;
			command.SingleChar = (ignoreCase ? char.ToUpper(pattern[i]) : pattern[i]);
		}
	}

	private bool Match(ITextBufferStrategy document, int offset, bool ignoreCase, int programStart)
	{
		int num = offset;
		curMatchEndOffset = -1;
		for (int i = programStart; i < patternProgram.Count; i++)
		{
			if (num >= document.Length)
			{
				return false;
			}
			char c = (ignoreCase ? char.ToUpper(document.GetCharAt(num)) : document.GetCharAt(num));
			Command command = (Command)patternProgram[i];
			switch (command.CommandType)
			{
			case CommandType.Match:
				if (c != command.SingleChar)
				{
					return false;
				}
				break;
			case CommandType.AnyZeroOrMore:
				if (c == '\n')
				{
					return false;
				}
				if (!Match(document, num, ignoreCase, i + 1))
				{
					return Match(document, num + 1, ignoreCase, i);
				}
				return true;
			case CommandType.AnyDigit:
				if (!char.IsDigit(c) && c != '#')
				{
					return false;
				}
				break;
			case CommandType.AnyInList:
				if (command.CharList.IndexOf(c) < 0)
				{
					return false;
				}
				break;
			case CommandType.NoneInList:
				if (command.CharList.IndexOf(c) >= 0)
				{
					return false;
				}
				break;
			}
			num++;
		}
		curMatchEndOffset = num;
		return true;
	}

	private int InternalFindNext(ITextIterator textIterator)
	{
		while (textIterator.MoveAhead(1))
		{
			int position = textIterator.Position;
			if (Match(textIterator.TextBuffer, position, !SearchOptions.MatchCase, 0) && (!SearchOptions.MatchWholeWord || SearchReplaceUtilities.IsWholeWordAt(textIterator.TextBuffer, position, curMatchEndOffset - position)))
			{
				textIterator.MoveAhead(curMatchEndOffset - position - 1);
				return position;
			}
		}
		return -1;
	}

	private int InternalFindNext(ITextIterator textIterator, int offset, int length)
	{
		while (textIterator.MoveAhead(1) && TextSelection.IsInsideRange(textIterator.Position, offset, length))
		{
			int position = textIterator.Position;
			if (Match(textIterator.TextBuffer, position, !SearchOptions.MatchCase, 0) && (!SearchOptions.MatchWholeWord || SearchReplaceUtilities.IsWholeWordAt(textIterator.TextBuffer, position, curMatchEndOffset - position)))
			{
				if (TextSelection.IsInsideRange(curMatchEndOffset - 1, offset, length))
				{
					textIterator.MoveAhead(curMatchEndOffset - position - 1);
					return position;
				}
				return -1;
			}
		}
		return -1;
	}

	public bool CompilePattern(IProgressNotificationTaskInstance monitor)
	{
		CompilePattern(SearchOptions.FindPattern, !SearchOptions.MatchCase);
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
		return new SearchResult(offset, curMatchEndOffset - offset);
	}
}
