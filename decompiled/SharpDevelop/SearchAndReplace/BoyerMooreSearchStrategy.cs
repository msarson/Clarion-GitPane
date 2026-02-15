using System.Collections.Generic;

namespace SearchAndReplace;

public class BoyerMooreSearchStrategy
{
	private Dictionary<char, int[]> patternCharShifts;

	private int[] otherCharShifts;

	private string searchPattern;

	private int patternLength;

	public void CompilePattern()
	{
		searchPattern = (SearchOptions.MatchCase ? SearchOptions.FindPattern : SearchOptions.FindPattern.ToUpper());
		patternLength = searchPattern.Length;
		int num = patternLength;
		patternCharShifts = new Dictionary<char, int[]>();
		for (int i = 0; i < patternLength; i++)
		{
			if (!patternCharShifts.ContainsKey(searchPattern[i]))
			{
				patternCharShifts.Add(searchPattern[i], new int[patternLength]);
			}
		}
		otherCharShifts = new int[patternLength];
		foreach (KeyValuePair<char, int[]> patternCharShift in patternCharShifts)
		{
			patternCharShift.Value[patternLength - 1] = num;
		}
		otherCharShifts[patternLength - 1] = num;
		for (int num2 = patternLength - 1; num2 >= 0; num2--)
		{
			string text = new string(searchPattern.ToCharArray(), num2 + 1, patternLength - num2 - 1);
			if (searchPattern.StartsWith(text))
			{
				num = num2 + 1;
			}
			otherCharShifts[num2] = num;
			string text2 = new string(searchPattern.ToCharArray(), 0, searchPattern.Length - 1);
			if (text2.LastIndexOf(text) > 0 || text.Length == 0)
			{
				foreach (KeyValuePair<char, int[]> patternCharShift2 in patternCharShifts)
				{
					string value = patternCharShift2.Key + text;
					int num3 = text2.LastIndexOf(value);
					if (num3 >= 0)
					{
						patternCharShift2.Value[num2] = num2 - num3;
					}
					else
					{
						patternCharShift2.Value[num2] = num;
					}
					if (patternCharShift2.Key == searchPattern[num2])
					{
						patternCharShift2.Value[num2] = 0;
					}
				}
			}
			else
			{
				foreach (KeyValuePair<char, int[]> patternCharShift3 in patternCharShifts)
				{
					patternCharShift3.Value[num2] = num;
					if (patternCharShift3.Key == searchPattern[num2])
					{
						patternCharShift3.Value[num2] = 0;
					}
				}
			}
		}
	}

	private int InternalFindNext(ITextIterator textIterator)
	{
		return -1;
	}

	public SearchResult FindNext(ITextIterator textIterator)
	{
		int num = InternalFindNext(textIterator);
		if (num < 0)
		{
			return null;
		}
		return new SearchResult(num, searchPattern.Length);
	}
}
