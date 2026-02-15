using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common;

public abstract class ClaHighlightingStrategy : DefaultHighlightingStrategy
{
	private static Regex numericPict = new Regex("^@N(\\$|(~[^~]*?~))?(-|(?<cazzoPar>\\())?(0|_|\\*)?[0-9]+(-|_|(?<cazzoDot>\\.))?(?(cazzoDot)((v|`|\\.)?[0-9]+)|((v|`|\\.)[0-9]+))?(\\$|~[^~]*?~)?(?(cazzoPar)\\)|-?)B?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	private static Regex scientificPict = new Regex("^@E(?<numa>[0-9]+)(\\.|_|`)\\.?(?<numb>[0-9]+)B?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	private static Regex stringPict = new Regex("^@S(?<size>\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	private static Regex datePict = new Regex("^@D(0|\\*)?(?<num>([1-9][0-9]?))(\\.|`|-|_)?((<|>)(?<range>[0-9]*))?B?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	private static Regex timePict = new Regex("^@T0?[1-8](\\.|`|-|_)?B?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	private static Regex patternPict1 = new Regex("^@P[^P]*P(B|b)?", RegexOptions.Compiled | RegexOptions.Singleline);

	private static Regex patternPict2 = new Regex("^@p[^p]*p(B|b)?", RegexOptions.Compiled | RegexOptions.Singleline);

	private static Regex keyPict1 = new Regex("^@K[^K]*K(B|b)?", RegexOptions.Compiled | RegexOptions.Singleline);

	private static Regex keyPict2 = new Regex("^@k[^k]*k(B|b)?", RegexOptions.Compiled | RegexOptions.Singleline);

	public ClaHighlightingStrategy(string language)
		: base(language)
	{
	}

	public abstract bool IsHardLanguageKeyword(string text);

	public abstract int GetLabelLength(string text);

	protected override bool OverrideSpan(string spanBegin, IDocument document, List<TextWord> words, Span span, ref int lineOffset)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		if (spanBegin == "@")
		{
			string text = document.GetText((ISegment)(object)base.currentLine);
			if ((lineOffset + 1 < text.Length && text[lineOffset + 1] == '@') || (lineOffset > 0 && text[lineOffset - 1] == '@'))
			{
				base.currentLength++;
				return true;
			}
			text = text.Substring(lineOffset);
			int num = IsValidPicture(text);
			if (num > 0)
			{
				words.Add(new TextWord(document, base.currentLine, base.currentOffset, num, span.BeginColor, false));
				base.currentOffset += num;
				base.currentLength = 0;
				lineOffset += num - 1;
				return true;
			}
		}
		return false;
	}

	protected override HighlightColor GetColor(HighlightRuleSet ruleSet, IDocument document, LineSegment currentSegment, int currentOffset, int currentLength)
	{
		string text = document.GetText((ISegment)(object)currentSegment);
		if (currentOffset > 0 && (text[currentOffset - 1] == ':' || text[currentOffset - 1] == '.'))
		{
			return null;
		}
		if (currentOffset + currentLength < currentSegment.Length)
		{
			if (text[currentOffset + currentLength] == ':')
			{
				return null;
			}
			if (text[currentOffset + currentLength] == '.')
			{
				string text2 = text.Substring(currentOffset, currentLength);
				if (!IsHardLanguageKeyword(text2) && !text2.Equals("self", StringComparison.InvariantCultureIgnoreCase) && !text2.Equals("parent", StringComparison.InvariantCultureIgnoreCase))
				{
					return null;
				}
			}
		}
		return ((DefaultHighlightingStrategy)this).GetColor(ruleSet, document, currentSegment, currentOffset, currentLength);
	}

	protected override HighlightColor GetContextDigitColor(IDocument document, LineSegment currentSegment, int currentOffset, int currentLength)
	{
		string text = document.GetText((ISegment)(object)currentSegment);
		if (currentOffset > 0 && text[currentOffset - 1] == ':')
		{
			return ((DefaultHighlightingStrategy)this).DefaultTextColor;
		}
		if (currentOffset + currentLength + 1 < currentSegment.Length && text[currentOffset + currentLength + 1] == ':')
		{
			return ((DefaultHighlightingStrategy)this).DefaultTextColor;
		}
		return ((DefaultHighlightingStrategy)this).DigitColor;
	}

	protected override void OnParsedLine(IDocument document, LineSegment currentLine, List<TextWord> words)
	{
		string text = "BlockComment";
		if (base.activeSpan != null && base.activeSpan.Name == text)
		{
			return;
		}
		string text2 = document.GetText((ISegment)(object)currentLine);
		LineSegment val = ((base.currentLineNumber > 0) ? document.GetLineSegment(base.currentLineNumber - 1) : null);
		bool flag = val != null && val.HighlightSpanStack != null && !val.HighlightSpanStack.IsEmpty && val.HighlightSpanStack.Peek().Name == text;
		int labelLength = GetLabelLength(text2);
		if (labelLength <= 0 || flag)
		{
			return;
		}
		for (int i = 0; i < words.Count; i++)
		{
			if (words[i].Offset < labelLength)
			{
				words[i].SyntaxColor = ((DefaultHighlightingStrategy)this).GetColorFor("Label");
			}
		}
	}

	private static int IsValidPicture(string pictureText)
	{
		Match match = numericPict.Match(pictureText);
		if (!match.Success)
		{
			match = scientificPict.Match(pictureText);
			if (!match.Success)
			{
				match = stringPict.Match(pictureText);
				if (!match.Success)
				{
					match = datePict.Match(pictureText);
					if (!match.Success)
					{
						match = timePict.Match(pictureText);
						if (!match.Success)
						{
							match = patternPict1.Match(pictureText);
							if (!match.Success)
							{
								match = patternPict2.Match(pictureText);
								if (!match.Success)
								{
									match = keyPict1.Match(pictureText);
									if (!match.Success)
									{
										match = keyPict2.Match(pictureText);
									}
								}
							}
						}
					}
				}
			}
		}
		if (match.Success)
		{
			return match.Length;
		}
		return -1;
	}
}
