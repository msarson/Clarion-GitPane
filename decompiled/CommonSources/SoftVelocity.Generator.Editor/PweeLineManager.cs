using System;
using System.Collections.Generic;
using System.Drawing;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Generator.PWEE;

namespace SoftVelocity.Generator.Editor;

public class PweeLineManager : ICustomLineManager
{
	private List<CustomLine> lines = new List<CustomLine>();

	private CustomPweeLine lastEmbed;

	private CustomPweeLine firstEmbed;

	private SortedList<int, CustomLine> filledEmbeds = new SortedList<int, CustomLine>();

	private IDocument document;

	private bool disableDocumentMonitor;

	private int oldLinesCount;

	private CustomPweeLine lastModifiedLine;

	public List<CustomLine> CustomLines => lines;

	public bool DisableDocumentMonitor
	{
		get
		{
			return disableDocumentMonitor;
		}
		set
		{
			disableDocumentMonitor = value;
		}
	}

	public CustomPweeLine LastEmbed => lastEmbed;

	public CustomPweeLine FirstEmbed => firstEmbed;

	public CustomPweeLine LastFilledEmbed
	{
		get
		{
			if (filledEmbeds.Count <= 0)
			{
				return null;
			}
			return (CustomPweeLine)(object)filledEmbeds.Values[filledEmbeds.Count - 1];
		}
	}

	public CustomPweeLine FirstFilledEmbed
	{
		get
		{
			if (filledEmbeds.Count <= 0)
			{
				return null;
			}
			return (CustomPweeLine)(object)filledEmbeds.Values[0];
		}
	}

	public event EventHandler BeforeChanged;

	public event EventHandler Changed;

	public PweeLineManager(IDocument doc)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		document = doc;
		doc.DocumentChanged += new DocumentEventHandler(DocumentChanged);
		doc.DocumentAboutToBeChanged += new DocumentEventHandler(DocumentAboutToBeChanged);
	}

	public CustomPweeLine GetPrevEmbed(int lineNr)
	{
		int num = FindCustomLineIndex(lines, lineNr);
		if (num >= 0)
		{
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				if (lines[num2] is CustomPweeLine customPweeLine && customPweeLine.PweePart is IPweeEmbedPoint)
				{
					return customPweeLine;
				}
			}
		}
		return null;
	}

	public CustomPweeLine GetNextEmbed(int lineNr)
	{
		int num = FindCustomLineIndex(lines, lineNr);
		if (num >= 0)
		{
			for (int i = num + 1; i < lines.Count; i++)
			{
				if (lines[i] is CustomPweeLine customPweeLine && customPweeLine.PweePart is IPweeEmbedPoint)
				{
					return customPweeLine;
				}
			}
		}
		return null;
	}

	public CustomPweeLine GetPrevFilledEmbed(int lineNr)
	{
		int num = FindCustomLineIndex(filledEmbeds.Values, lineNr);
		if (num > 0)
		{
			return (CustomPweeLine)(object)filledEmbeds.Values[num - 1];
		}
		if (num < 0)
		{
			int num2 = ~num - 1;
			if (num2 >= 0 && num2 < filledEmbeds.Count)
			{
				return (CustomPweeLine)(object)filledEmbeds.Values[num2];
			}
		}
		return null;
	}

	public CustomPweeLine GetNextFilledEmbed(int lineNr)
	{
		int num = FindCustomLineIndex(filledEmbeds.Values, lineNr);
		if (num >= 0)
		{
			if (num < filledEmbeds.Count - 1)
			{
				return (CustomPweeLine)(object)filledEmbeds.Values[num + 1];
			}
		}
		else if (num < 0)
		{
			int num2 = ~num;
			if (num2 >= 0 && num2 < filledEmbeds.Count)
			{
				return (CustomPweeLine)(object)filledEmbeds.Values[num2];
			}
		}
		return null;
	}

	private static int FindCustomLineIndex(IList<CustomLine> values, int nr)
	{
		int num = 0;
		int num2 = values.Count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			CustomLine val = values[num3];
			int num4;
			if (val.StartLineNr > nr)
			{
				num4 = 1;
			}
			else
			{
				if (val.EndLineNr >= nr)
				{
					return num3;
				}
				num4 = -1;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	public CustomPweeLine GetCustomLine(int lineNr)
	{
		int num = FindCustomLineIndex(lines, lineNr);
		if (num >= 0)
		{
			return lines[num] as CustomPweeLine;
		}
		return null;
	}

	public Color GetCustomColor(int lineNr, Color defaultColor)
	{
		int num = FindCustomLineIndex(lines, lineNr);
		if (num >= 0)
		{
			return lines[num].Color;
		}
		return defaultColor;
	}

	public bool IsReadOnly(int lineNr, bool defaultReadOnly)
	{
		int num = FindCustomLineIndex(lines, lineNr);
		if (num >= 0)
		{
			return lines[num].ReadOnly;
		}
		return defaultReadOnly;
	}

	public bool IsReadOnly(ISelection selection, bool defaultReadOnly)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		TextLocation startPosition = selection.StartPosition;
		int y = ((TextLocation)(ref startPosition)).Y;
		TextLocation endPosition = selection.EndPosition;
		int y2 = ((TextLocation)(ref endPosition)).Y;
		foreach (CustomLine line in lines)
		{
			if (line.ReadOnly && (y >= line.StartLineNr || y2 >= line.StartLineNr) && (y <= line.EndLineNr || y2 <= line.EndLineNr))
			{
				return true;
			}
		}
		return defaultReadOnly;
	}

	public void Clear()
	{
		OnBeforeChanged();
		lines.Clear();
		lastModifiedLine = null;
		oldLinesCount = 0;
		lastEmbed = null;
		firstEmbed = null;
		filledEmbeds.Clear();
		OnChanged();
	}

	private void OnChanged()
	{
		if (this.Changed != null)
		{
			this.Changed(this, null);
		}
	}

	private void OnBeforeChanged()
	{
		if (this.BeforeChanged != null)
		{
			this.BeforeChanged(this, null);
		}
	}

	public void AddCustomLine(IPweePart pweePart, int lineNr, Color customColor, bool readOnly)
	{
		OnBeforeChanged();
		InsertSorted(new CustomPweeLine(pweePart, lineNr, customColor, readOnly));
		OnChanged();
	}

	public void AddCustomLine(IPweePart pweePart, int startLineNr, int endLineNr, Color customColor, bool readOnly)
	{
		OnBeforeChanged();
		InsertSorted(new CustomPweeLine(pweePart, startLineNr, endLineNr, customColor, readOnly));
		OnChanged();
	}

	public void AddCustomLine(int lineNr, Color customColor, bool readOnly)
	{
		OnBeforeChanged();
		InsertSorted(new CustomPweeLine(null, lineNr, customColor, readOnly));
		OnChanged();
	}

	public void AddCustomLine(int startLineNr, int endLineNr, Color customColor, bool readOnly)
	{
		OnBeforeChanged();
		InsertSorted(new CustomPweeLine(null, startLineNr, endLineNr, customColor, readOnly));
		OnChanged();
	}

	private void InsertSorted(CustomPweeLine line)
	{
		if (lines.Count == 0 || lines[lines.Count - 1].StartLineNr < ((CustomLine)line).StartLineNr)
		{
			lines.Add((CustomLine)(object)line);
		}
		else
		{
			for (int i = 0; i < lines.Count; i++)
			{
				if (lines[i].StartLineNr > ((CustomLine)line).StartLineNr)
				{
					lines.Insert(i, (CustomLine)(object)line);
					break;
				}
			}
		}
		if (line.PweePart is IPweeEmbedPoint)
		{
			if (firstEmbed == null || ((CustomLine)firstEmbed).StartLineNr > ((CustomLine)line).StartLineNr)
			{
				firstEmbed = line;
			}
			if (lastEmbed == null || ((CustomLine)lastEmbed).StartLineNr < ((CustomLine)line).StartLineNr)
			{
				lastEmbed = line;
			}
			if (!string.IsNullOrEmpty(((IPweeEmbedPoint)line.PweePart).Text.Text) && !filledEmbeds.ContainsKey(((CustomLine)line).StartLineNr))
			{
				filledEmbeds.Add(((CustomLine)line).StartLineNr, (CustomLine)(object)line);
			}
		}
	}

	public void RemoveCustomLine(int lineNr)
	{
		for (int i = 0; i < lines.Count; i++)
		{
			if (lines[i].StartLineNr > lineNr || lines[i].EndLineNr < lineNr)
			{
				continue;
			}
			OnBeforeChanged();
			lines.RemoveAt(i);
			if (lines[i] is CustomPweeLine customPweeLine)
			{
				if (customPweeLine == firstEmbed)
				{
					firstEmbed = null;
					for (int j = i; j < lines.Count; j++)
					{
						if (lines[j] is CustomPweeLine customPweeLine2 && customPweeLine2.PweePart is IPweeEmbedPoint)
						{
							firstEmbed = customPweeLine2;
							break;
						}
					}
				}
				if (customPweeLine == lastEmbed)
				{
					lastEmbed = null;
					for (int num = i; num >= 0; num--)
					{
						if (lines[num] is CustomPweeLine customPweeLine3 && customPweeLine3.PweePart is IPweeEmbedPoint)
						{
							lastEmbed = customPweeLine3;
							break;
						}
					}
				}
				if (filledEmbeds.ContainsKey(((CustomLine)customPweeLine).StartLineNr))
				{
					CustomPweeLine customPweeLine4 = (CustomPweeLine)(object)filledEmbeds[((CustomLine)customPweeLine).StartLineNr];
					if (customPweeLine4 == customPweeLine)
					{
						filledEmbeds.Remove(((CustomLine)customPweeLine).StartLineNr);
					}
				}
			}
			OnChanged();
			break;
		}
	}

	private void DocumentAboutToBeChanged(object sender, DocumentEventArgs e)
	{
		if (!disableDocumentMonitor)
		{
			oldLinesCount = e.Document.TotalNumberOfLines;
		}
	}

	private void DocumentChanged(object sender, DocumentEventArgs e)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (disableDocumentMonitor)
		{
			return;
		}
		int lineNumberForOffset = e.Document.GetLineNumberForOffset(e.Offset);
		int num = e.Document.TotalNumberOfLines - oldLinesCount;
		if (num != 0)
		{
			MoveIndices(sender, new LineCountChangeEventArgs(e.Document, lineNumberForOffset, num));
		}
		if (lastModifiedLine == null || ((CustomLine)lastModifiedLine).StartLineNr > lineNumberForOffset || ((CustomLine)lastModifiedLine).EndLineNr < lineNumberForOffset)
		{
			lastModifiedLine = GetCustomLine(lineNumberForOffset);
			if (lastModifiedLine != null)
			{
				lastModifiedLine.Dirty = true;
			}
		}
		if (lastModifiedLine == null)
		{
			return;
		}
		string embedText = GetEmbedText(lastModifiedLine);
		if (embedText == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(embedText))
		{
			if (filledEmbeds.ContainsKey(((CustomLine)lastModifiedLine).StartLineNr))
			{
				CustomPweeLine customPweeLine = (CustomPweeLine)(object)filledEmbeds[((CustomLine)lastModifiedLine).StartLineNr];
				if (customPweeLine == lastModifiedLine)
				{
					filledEmbeds.Remove(((CustomLine)lastModifiedLine).StartLineNr);
				}
			}
		}
		else if (!filledEmbeds.ContainsKey(((CustomLine)lastModifiedLine).StartLineNr))
		{
			filledEmbeds.Add(((CustomLine)lastModifiedLine).StartLineNr, (CustomLine)(object)lastModifiedLine);
		}
	}

	private string GetEmbedText(CustomPweeLine line)
	{
		LineSegment lineSegment = document.GetLineSegment(((CustomLine)line).StartLineNr);
		LineSegment lineSegment2 = document.GetLineSegment(((CustomLine)line).EndLineNr);
		return document.GetText(lineSegment.Offset, lineSegment2.Offset + lineSegment2.Length - lineSegment.Offset);
	}

	private void MoveIndices(object sender, LineCountChangeEventArgs e)
	{
		bool flag = lines.Count > 0;
		if (flag)
		{
			OnBeforeChanged();
		}
		for (int i = 0; i < lines.Count; i++)
		{
			int startLineNr = lines[i].StartLineNr;
			int endLineNr = lines[i].EndLineNr;
			if (e.LineStart >= startLineNr)
			{
				if (e.LineStart <= endLineNr)
				{
					if (e.LinesMoved < 0 && e.LineStart - e.LinesMoved > endLineNr)
					{
						lines[i].EndLineNr = e.LineStart;
						continue;
					}
					CustomLine obj = lines[i];
					obj.EndLineNr += e.LinesMoved;
				}
			}
			else
			{
				CustomLine obj2 = lines[i];
				obj2.StartLineNr += e.LinesMoved;
				CustomLine obj3 = lines[i];
				obj3.EndLineNr += e.LinesMoved;
			}
		}
		if (flag)
		{
			OnChanged();
		}
	}
}
