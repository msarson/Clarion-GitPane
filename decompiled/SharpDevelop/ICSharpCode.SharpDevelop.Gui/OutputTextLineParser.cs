using System;
using System.Text.RegularExpressions;

namespace ICSharpCode.SharpDevelop.Gui;

public static class OutputTextLineParser
{
	public static FileLineReference GetCSharpCompilerFileLineReference(string lineText)
	{
		if (lineText != null)
		{
			Match match = Regex.Match(lineText, "\\b(\\w:[/\\\\].*?)\\((\\d+),(\\d+)\\)");
			if (match.Success)
			{
				try
				{
					int line = Convert.ToInt32(match.Groups[2].Value) - 1;
					int column = Convert.ToInt32(match.Groups[3].Value) - 1;
					return new FileLineReference(match.Groups[1].Value, line, column);
				}
				catch (FormatException)
				{
				}
				catch (OverflowException)
				{
				}
			}
		}
		return null;
	}

	public static FileLineReference GetFileLineReference(string lineText)
	{
		FileLineReference fileLineReference = GetCSharpCompilerFileLineReference(lineText);
		if (fileLineReference == null)
		{
			fileLineReference = GetNUnitOutputFileLineReference(lineText, multiline: false);
		}
		if (fileLineReference == null)
		{
			fileLineReference = GetCppCompilerFileLineReference(lineText);
		}
		return fileLineReference;
	}

	public static FileLineReference GetNUnitOutputFileLineReference(string lineText, bool multiline)
	{
		RegexOptions options = (multiline ? RegexOptions.Multiline : RegexOptions.None);
		FileLineReference result = null;
		if (lineText != null)
		{
			Match match = Regex.Match(lineText, "\\sin\\s(.*?):line\\s(\\d+)?\\r?$", options);
			while (match.Success)
			{
				try
				{
					int line = Convert.ToInt32(match.Groups[2].Value) - 1;
					result = new FileLineReference(match.Groups[1].Value, line);
				}
				catch (FormatException)
				{
				}
				catch (OverflowException)
				{
				}
				match = match.NextMatch();
			}
		}
		return result;
	}

	public static FileLineReference GetCppCompilerFileLineReference(string lineText)
	{
		if (lineText != null)
		{
			Match match = Regex.Match(lineText, "\\b(\\w:[/\\\\].*?)\\((\\d+)\\)");
			if (match.Success)
			{
				try
				{
					int line = Convert.ToInt32(match.Groups[2].Value) - 1;
					return new FileLineReference(match.Groups[1].Value.Trim(), line);
				}
				catch (FormatException)
				{
				}
				catch (OverflowException)
				{
				}
			}
		}
		return null;
	}
}
