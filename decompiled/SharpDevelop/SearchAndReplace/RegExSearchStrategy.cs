using System;
using System.Text.RegularExpressions;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class RegExSearchStrategy : ISearchStrategy
{
	private class RegexSearchResult : SearchResult
	{
		private Match m;

		private Regex escapeSequences = new Regex("(?<=^|[^\\\\])((?<cr>\\\\r)|(?<nl>\\\\n)|(?<tab>\\\\t))", RegexOptions.Compiled | RegexOptions.Singleline);

		internal RegexSearchResult(Match m)
			: base(m.Index, m.Length)
		{
			this.m = m;
		}

		public override string TransformReplacePattern(string pattern)
		{
			return escapeSequences.Replace(m.Result(pattern), ReplaceEscapeSequence);
		}

		private static string ReplaceEscapeSequence(Match m)
		{
			if (m.Groups["cr"].Success)
			{
				return "\r";
			}
			if (m.Groups["nl"].Success)
			{
				return "\n";
			}
			if (m.Groups["tab"].Success)
			{
				return "\t";
			}
			return m.Value;
		}
	}

	private Regex regex;

	private IProgressNotificationTaskInstance monitor;

	private bool multiLine = true;

	private string document;

	public bool CompilePattern(IProgressNotificationTaskInstance monitor)
	{
		this.monitor = monitor;
		multiLine = SearchOptions.MultiLineMatch;
		RegexOptions regexOptions = RegexOptions.Compiled;
		if (!SearchOptions.MatchCase)
		{
			regexOptions |= RegexOptions.IgnoreCase;
		}
		try
		{
			regex = new Regex(SearchOptions.FindPattern, regexOptions);
			return true;
		}
		catch (ArgumentException ex)
		{
			MessageService.ShowError("${res:Dialog.NewProject.SearchReplace.ErrorParsingRegex}\n" + ex.Message);
			return false;
		}
	}

	public SearchResult FindNext(ITextIterator textIterator)
	{
		if (multiLine)
		{
			document = textIterator.TextBuffer.GetText(0, textIterator.TextBuffer.Length);
			if (textIterator.MoveAhead(1))
			{
				if (monitor != null && monitor.IsCancelled)
				{
					return null;
				}
				Match match = regex.Match(document, textIterator.Position);
				if (match == null || match.Index <= 0 || match.Length <= 0)
				{
					document = null;
					return null;
				}
				int num = match.Index - textIterator.Position;
				if (num <= 0 || textIterator.MoveAhead(num))
				{
					document = null;
					return new RegexSearchResult(match);
				}
				document = null;
				return null;
			}
		}
		else
		{
			if (document == null)
			{
				document = textIterator.TextBuffer.GetText(0, textIterator.TextBuffer.Length);
			}
			int num2 = 0;
			int num3 = 0;
			while (textIterator.MoveAhead(num2 + 2))
			{
				num3 = document.IndexOf("\r\n", textIterator.Position);
				if (num3 <= 0)
				{
					num3 = textIterator.TextBuffer.Length;
				}
				num2 = num3 - textIterator.Position;
				Match match2 = null;
				if (num2 > 0)
				{
					string input = document.Substring(textIterator.Position, num2);
					match2 = regex.Match(input);
					if (match2 != null && match2.Success)
					{
						int position = textIterator.Position;
						textIterator.MoveAhead(num2 + 2);
						return new SearchResult(position, num2);
					}
				}
			}
		}
		document = null;
		return null;
	}

	public SearchResult FindNext(ITextIterator textIterator, int offset, int length)
	{
		string text = textIterator.TextBuffer.GetText(0, textIterator.TextBuffer.Length);
		while (textIterator.MoveAhead(1) && TextSelection.IsInsideRange(textIterator.Position, offset, length))
		{
			Match match = regex.Match(text, textIterator.Position);
			if (match == null || match.Index <= 0 || match.Length <= 0)
			{
				continue;
			}
			int num = match.Index - textIterator.Position;
			if (num <= 0 || textIterator.MoveAhead(num))
			{
				if (TextSelection.IsInsideRange(match.Index + match.Length - 1, offset, length))
				{
					return new RegexSearchResult(match);
				}
				return null;
			}
			return null;
		}
		return null;
	}
}
