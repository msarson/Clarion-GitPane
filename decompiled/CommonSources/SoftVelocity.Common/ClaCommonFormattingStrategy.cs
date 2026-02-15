using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common;

public abstract class ClaCommonFormattingStrategy : DefaultFormattingStrategy
{
	protected enum LineType
	{
		None = 0,
		EmptyLine = 1,
		Comment = 2,
		MLCommentPart = 3,
		End = 4,
		Program = 5,
		Member = 6,
		Include = 7,
		Pragma = 8,
		Map = 9,
		Code = 10,
		Data = 11,
		Procedure = 12,
		Function = 13,
		Window = 14,
		Application = 15,
		Class = 16,
		File = 17,
		View = 18,
		Group = 19,
		Queue = 20,
		Report = 21,
		Interface = 22,
		Routine = 23,
		Accept = 24,
		Begin = 25,
		Try = 26,
		Finally = 27,
		Record = 28,
		Header = 29,
		Detail = 30,
		Option = 31,
		Break = 32,
		Footer = 33,
		Join = 34,
		Menu = 35,
		Menubar = 36,
		Toolbar = 37,
		Sheet = 38,
		Tab = 39,
		Ole = 40,
		Form = 41,
		Case = 42,
		If = 43,
		ElsIf = 44,
		Else = 45,
		Loop = 46,
		Of = 47,
		OrOf = 48,
		Execute = 49,
		Catch = 50,
		Module = 51,
		While = 52,
		Until = 53,
		Itemize = 54,
		Section = 55,
		Namespace = 200,
		Using = 201,
		Inline = 202,
		Getter = 203,
		Setter = 204,
		Property = 205,
		Indexer = 206,
		Struct = 207,
		Enum = 208,
		Foreach = 209,
		Checked = 210,
		Unchecked = 211,
		Synclock = 212
	}

	protected class LineInfo
	{
		private bool mlCommentBegin;

		private bool mlCommentEnd;

		private bool blockEnded;

		private bool lineContinued;

		private LineType lineType;

		private string processedLineText;

		private string labelText;

		private int keywordIndex;

		private int parentIndex = -1;

		private int bracketsCounter;

		public bool MLCommentBegin
		{
			[DebuggerStepThrough]
			get
			{
				return mlCommentBegin;
			}
			[DebuggerStepThrough]
			set
			{
				mlCommentBegin = value;
			}
		}

		public bool MLCommentEnd
		{
			[DebuggerStepThrough]
			get
			{
				return mlCommentEnd;
			}
			[DebuggerStepThrough]
			set
			{
				mlCommentEnd = value;
			}
		}

		public bool BlockEnded
		{
			[DebuggerStepThrough]
			get
			{
				return blockEnded;
			}
			[DebuggerStepThrough]
			set
			{
				blockEnded = value;
			}
		}

		public bool LineContinued
		{
			[DebuggerStepThrough]
			get
			{
				return lineContinued;
			}
			[DebuggerStepThrough]
			set
			{
				lineContinued = value;
			}
		}

		public LineType Type
		{
			[DebuggerStepThrough]
			get
			{
				return lineType;
			}
			[DebuggerStepThrough]
			set
			{
				lineType = value;
			}
		}

		public string ProcessedLineText
		{
			[DebuggerStepThrough]
			get
			{
				return processedLineText;
			}
			[DebuggerStepThrough]
			set
			{
				processedLineText = value;
			}
		}

		public string LabelText
		{
			[DebuggerStepThrough]
			get
			{
				return labelText;
			}
			[DebuggerStepThrough]
			set
			{
				labelText = value;
			}
		}

		public int KeywordIndex
		{
			[DebuggerStepThrough]
			get
			{
				return keywordIndex;
			}
			[DebuggerStepThrough]
			set
			{
				keywordIndex = value;
			}
		}

		public int ParentIndex
		{
			[DebuggerStepThrough]
			get
			{
				return parentIndex;
			}
			[DebuggerStepThrough]
			set
			{
				parentIndex = value;
			}
		}

		public int BracketsCounter
		{
			[DebuggerStepThrough]
			get
			{
				return bracketsCounter;
			}
			[DebuggerStepThrough]
			set
			{
				bracketsCounter = value;
			}
		}

		public void Initialize()
		{
			mlCommentBegin = false;
			mlCommentEnd = false;
			blockEnded = false;
			lineContinued = false;
			lineType = LineType.None;
			processedLineText = null;
			keywordIndex = 0;
			labelText = null;
			parentIndex = -1;
			bracketsCounter = 0;
		}

		public char GetLastMeaningChar(out int charIndex)
		{
			if (string.IsNullOrEmpty(processedLineText))
			{
				charIndex = -1;
				return '\0';
			}
			string text = processedLineText.TrimEnd();
			charIndex = text.Length - 1;
			return text[charIndex];
		}
	}

	private class AffectedLinesBlock
	{
		private int beginLine;

		private List<string> linesText = new List<string>();

		public int BeginLine => beginLine;

		public int EndLine => beginLine + linesText.Count - 1;

		public ReadOnlyCollection<string> TextLines => linesText.AsReadOnly();

		public AffectedLinesBlock(int bLine, string lineText)
		{
			beginLine = bLine;
			linesText.Add(lineText);
		}

		public void AddLineText(string lineText)
		{
			linesText.Add(lineText);
		}
	}

	protected const int CWTypesRegionBegin = 100;

	protected const int CWTypesRegionEnd = 199;

	protected const int CNTypesRegionBegin = 200;

	protected const int CNTypesRegionEnd = 299;

	protected static Dictionary<string, LineType> strToKeywordsEnum;

	private int minimalIndent = 1;

	private SmartFormatterOptions options;

	protected List<LineInfo> parsedLines;

	protected int curParentIndex;

	protected bool initialized;

	protected bool monitorDocChanges = true;

	private int affectedLineNum;

	private int deletedLinesCount;

	private bool pastingOperation;

	private int pastedLineNum = -1;

	private int pastedLinesCount = -1;

	protected static Regex ENDend;

	protected static Regex WHILEend;

	protected static Regex UNTILend;

	protected static Regex replaceStrings;

	protected static Regex replaceSLComment1;

	protected static Regex replaceSLComment2;

	protected static Regex replaceMLComment;

	protected static Regex replaceNumericPicture;

	protected static Regex replacePatternPicture1;

	protected static Regex replacePatternPicture2;

	protected static Regex replaceKeyPicture1;

	protected static Regex replaceKeyPicture2;

	protected static Regex codeLabelExpr;

	public abstract bool SupportsMLComments { get; }

	public abstract bool AutoInsertLineContinuation { get; }

	public abstract string CharsForValidLineEnd { get; }

	public abstract bool IsWin { get; }

	public bool Pasting
	{
		set
		{
			pastingOperation = value;
			pastedLineNum = -1;
			pastedLinesCount = -1;
		}
	}

	public bool Disposed => !initialized;

	protected SmartFormatterOptions Options => options ?? SmartFormatterOptions.General;

	public int MinimalIndent
	{
		get
		{
			return minimalIndent;
		}
		set
		{
			minimalIndent = value;
		}
	}

	protected int DefaultIndentColumn => MinimalIndent * Options.IndentSize + 1;

	static ClaCommonFormattingStrategy()
	{
		ENDend = new Regex("( |\\t|;)END$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		WHILEend = new Regex("(^|(;( |\\t)*))WHILE(\\s|\\()", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		UNTILend = new Regex("(^|(;( |\\t)*))UNTIL(\\s|\\()", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		replaceStrings = new Regex("'[^']*'", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		replaceSLComment1 = new Regex("!.*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		replaceSLComment2 = new Regex("(^|[^~])!([^~].*$|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		replaceMLComment = new Regex("!~(.)*?~!", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		replaceNumericPicture = new Regex("@N(\\$|(~[^~]*?~))?(-|(?<cazzoPar>\\())?(0|_|\\*)?[0-9]+(-|_|(?<cazzoDot>\\.))?(?(cazzoDot)((v|`|\\.)?[0-9]+)|((v|`|\\.)[0-9]+))?(?(cazzoPar)\\)|-?)($$|~[^~]*?~)?B?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		replacePatternPicture1 = new Regex("@P[^P]*PB?", RegexOptions.Compiled | RegexOptions.Singleline);
		replacePatternPicture2 = new Regex("@p[^p]*pB?", RegexOptions.Compiled | RegexOptions.Singleline);
		replaceKeyPicture1 = new Regex("@K[^K]*KB?", RegexOptions.Compiled | RegexOptions.Singleline);
		replaceKeyPicture2 = new Regex("@k[^k]*kB?", RegexOptions.Compiled | RegexOptions.Singleline);
		codeLabelExpr = new Regex("^\\??[A-Za-z_](\\w|:)*:$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
		strToKeywordsEnum = new Dictionary<string, LineType>(StringComparer.InvariantCultureIgnoreCase);
		for (LineType lineType = LineType.End; lineType <= LineType.Section; lineType++)
		{
			strToKeywordsEnum[lineType.ToString()] = lineType;
		}
		strToKeywordsEnum["."] = LineType.End;
		for (LineType lineType2 = LineType.Namespace; lineType2 <= LineType.Synclock; lineType2++)
		{
			strToKeywordsEnum[lineType2.ToString()] = lineType2;
		}
		string input = "Test Text";
		ENDend.IsMatch(input);
		WHILEend.IsMatch(input);
		UNTILend.IsMatch(input);
		replaceStrings.IsMatch(input);
		replaceSLComment1.IsMatch(input);
		replaceSLComment2.IsMatch(input);
		replaceMLComment.IsMatch(input);
		replaceNumericPicture.IsMatch(input);
		replacePatternPicture1.IsMatch(input);
		replacePatternPicture2.IsMatch(input);
		replaceKeyPicture1.IsMatch(input);
		replaceKeyPicture2.IsMatch(input);
		codeLabelExpr.IsMatch(input);
	}

	public virtual void InitializeParser(IDocument document)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (!initialized)
		{
			initialized = true;
			document.DocumentChanged += new DocumentEventHandler(DocumentChanged);
			document.DocumentAboutToBeChanged += new DocumentEventHandler(DocumentAboutToBeChanged);
		}
	}

	public virtual void DisposeParser(IDocument document)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (initialized)
		{
			initialized = false;
			document.DocumentChanged -= new DocumentEventHandler(DocumentChanged);
			document.DocumentAboutToBeChanged -= new DocumentEventHandler(DocumentAboutToBeChanged);
		}
	}

	public void OverrideOptions(SmartFormatterOptions o)
	{
		options = o;
	}

	private void DocumentAboutToBeChanged(object sender, DocumentEventArgs e)
	{
		if (!monitorDocChanges || parsedLines == null)
		{
			return;
		}
		deletedLinesCount = 0;
		if (e.Length == -1)
		{
			return;
		}
		LineSegment lineSegmentForOffset = e.Document.GetLineSegmentForOffset(e.Offset);
		int num = lineSegmentForOffset.TotalLength - (e.Offset - lineSegmentForOffset.Offset);
		affectedLineNum = e.Document.GetLineNumberForOffset(lineSegmentForOffset.Offset);
		if (num <= e.Length)
		{
			int num2 = affectedLineNum + 1;
			int num3 = num2;
			int num4 = e.Length - num;
			while (num4 >= 0 && num2 < parsedLines.Count)
			{
				parsedLines.RemoveAt(num2);
				lineSegmentForOffset = e.Document.GetLineSegment(num3);
				num4 -= lineSegmentForOffset.TotalLength;
				num3++;
				deletedLinesCount--;
			}
		}
	}

	private void DocumentChanged(object sender, DocumentEventArgs e)
	{
		if (!monitorDocChanges || parsedLines == null)
		{
			return;
		}
		if (e.Text != null && e.Length == 0 && e.Offset == 0)
		{
			ParseDocument(e.Document);
		}
		else if (e.Text != null)
		{
			curParentIndex = -1;
			LineSegment lineSegmentForOffset = e.Document.GetLineSegmentForOffset(e.Offset);
			int num = lineSegmentForOffset.TotalLength - (e.Offset - lineSegmentForOffset.Offset);
			int num2 = e.Document.GetLineNumberForOffset(lineSegmentForOffset.Offset);
			if (num > e.Text.Length)
			{
				ParseLine(num2, e.Document.GetText((ISegment)(object)lineSegmentForOffset));
				ChangeParentIndexes(num2 + 1, 0);
			}
			else
			{
				if (pastingOperation)
				{
					pastedLineNum = num2;
				}
				int num3 = e.Text.Length - num;
				ParseLine(num2, e.Document.GetText((ISegment)(object)lineSegmentForOffset));
				int num4 = 0;
				while (num3 >= 0 && num2 + 1 < e.Document.TotalNumberOfLines)
				{
					num2++;
					num4++;
					parsedLines.Insert(num2, new LineInfo());
					lineSegmentForOffset = e.Document.GetLineSegment(num2);
					ParseLine(num2, e.Document.GetText((ISegment)(object)lineSegmentForOffset));
					num3 -= lineSegmentForOffset.TotalLength;
				}
				ChangeParentIndexes(num2 + 1, num4 + deletedLinesCount);
				if (pastingOperation)
				{
					pastedLinesCount = num4 + 1;
				}
			}
			curParentIndex = -1;
		}
		else
		{
			curParentIndex = -1;
			ParseLine(affectedLineNum, e.Document.GetText((ISegment)(object)e.Document.GetLineSegment(affectedLineNum)));
			ChangeParentIndexes(affectedLineNum + 1, deletedLinesCount);
		}
	}

	private void ChangeParentIndexes(int lineNum, int newLinesNum)
	{
		curParentIndex = -1;
		while (lineNum < parsedLines.Count)
		{
			LineInfo li = parsedLines[lineNum];
			ManageLineParent(lineNum, li);
			lineNum++;
		}
	}

	public void GetPastedLines(ref int lineNum, ref int lineCount)
	{
		lineNum = pastedLineNum;
		lineCount = pastedLinesCount;
	}

	public void ParseDocument(IDocument document)
	{
		if (document != null)
		{
			if (parsedLines == null)
			{
				parsedLines = new List<LineInfo>();
			}
			else
			{
				parsedLines.Clear();
			}
			curParentIndex = -1;
			for (int i = 0; i < document.TotalNumberOfLines; i++)
			{
				parsedLines.Add(new LineInfo());
				LineSegment lineSegment = document.GetLineSegment(i);
				ParseLine(i, document.GetText((ISegment)(object)lineSegment));
			}
			curParentIndex = -1;
		}
	}

	protected virtual void ParseLine(int lineNum, string lineText)
	{
		LineInfo lineInfo = parsedLines[lineNum];
		lineInfo.Initialize();
		if (SupportsMLComments && lineNum > 0)
		{
			LineInfo lineInfo2 = parsedLines[lineNum - 1];
			if (lineInfo2.Type == LineType.MLCommentPart || lineInfo2.MLCommentBegin)
			{
				int num = lineText.IndexOf("~!");
				if (num == -1)
				{
					lineInfo.Type = LineType.MLCommentPart;
					lineInfo.ProcessedLineText = lineText;
					ManageLineParent(lineNum, lineInfo);
					return;
				}
				lineText = new string(' ', num + 2) + lineText.Substring(num + 2);
				lineInfo.MLCommentEnd = true;
			}
		}
		lineInfo.ProcessedLineText = ReplaceStringsAndComments(lineText);
		if (lineInfo.ProcessedLineText.Trim().Length == 0)
		{
			if (lineText.Trim().Length == 0 && !lineInfo.MLCommentEnd)
			{
				lineInfo.Type = LineType.EmptyLine;
			}
			else
			{
				lineInfo.Type = LineType.Comment;
			}
			ManageLineParent(lineNum, lineInfo);
			return;
		}
		if (SupportsMLComments)
		{
			int num2 = lineInfo.ProcessedLineText.IndexOf("!~");
			if (num2 != -1)
			{
				lineInfo.MLCommentBegin = true;
				lineInfo.ProcessedLineText = lineInfo.ProcessedLineText.Substring(0, num2).TrimEnd();
				if (lineInfo.ProcessedLineText.Length == 0)
				{
					lineInfo.Type = LineType.Comment;
					ManageLineParent(lineNum, lineInfo);
					return;
				}
			}
		}
		SetLineType(lineInfo);
		CheckBlockEndAndLineContinue(lineInfo);
		ManageLineParent(lineNum, lineInfo);
		CalculateBrackets(lineInfo);
	}

	protected abstract void CalculateBrackets(LineInfo li);

	protected static void CheckBlockEndAndLineContinue(LineInfo line)
	{
		string text = line.ProcessedLineText.Trim();
		if (line.Type == LineType.End)
		{
			line.BlockEnded = true;
		}
		else if (line.Type == LineType.While || line.Type == LineType.Until)
		{
			line.BlockEnded = true;
		}
		else if (text.EndsWith("."))
		{
			line.BlockEnded = true;
		}
		else if (ENDend.IsMatch(text))
		{
			line.BlockEnded = true;
		}
		else if (line.Type == LineType.Loop)
		{
			if (WHILEend.IsMatch(text))
			{
				line.BlockEnded = true;
			}
			else if (UNTILend.IsMatch(text))
			{
				line.BlockEnded = true;
			}
		}
		if (text.EndsWith("|"))
		{
			line.LineContinued = true;
		}
	}

	protected void ManageLineParent(int lineNum, LineInfo li)
	{
		if (curParentIndex == -1)
		{
			if (lineNum == 0)
			{
				switch (li.Type)
				{
				case LineType.None:
				case LineType.EmptyLine:
				case LineType.Comment:
				case LineType.MLCommentPart:
				case LineType.End:
				case LineType.Program:
				case LineType.Member:
				case LineType.Include:
				case LineType.Pragma:
				case LineType.ElsIf:
				case LineType.Else:
				case LineType.Of:
				case LineType.OrOf:
				case LineType.Catch:
				case LineType.While:
				case LineType.Until:
				case LineType.Section:
				case LineType.Namespace:
				case LineType.Using:
					return;
				}
				curParentIndex = lineNum;
				return;
			}
			switch (parsedLines[lineNum - 1].Type)
			{
			case LineType.None:
			case LineType.EmptyLine:
			case LineType.Comment:
			case LineType.MLCommentPart:
			case LineType.End:
			case LineType.Program:
			case LineType.Member:
			case LineType.Include:
			case LineType.Pragma:
			case LineType.ElsIf:
			case LineType.Else:
			case LineType.Of:
			case LineType.OrOf:
			case LineType.Catch:
			case LineType.While:
			case LineType.Until:
			case LineType.Section:
			case LineType.Namespace:
			case LineType.Using:
				if (!parsedLines[lineNum - 1].BlockEnded)
				{
					curParentIndex = parsedLines[lineNum - 1].ParentIndex;
				}
				else if (parsedLines[lineNum - 1].ParentIndex != -1)
				{
					curParentIndex = parsedLines[parsedLines[lineNum - 1].ParentIndex].ParentIndex;
				}
				break;
			case LineType.Procedure:
			case LineType.Function:
			case LineType.Routine:
			case LineType.Property:
			case LineType.Indexer:
				switch ((parsedLines[lineNum - 1].ParentIndex != -1) ? parsedLines[parsedLines[lineNum - 1].ParentIndex].Type : LineType.None)
				{
				case LineType.Map:
				case LineType.Class:
				case LineType.Interface:
				case LineType.Module:
					curParentIndex = parsedLines[lineNum - 1].ParentIndex;
					break;
				default:
					curParentIndex = lineNum - 1;
					break;
				}
				break;
			default:
				if (parsedLines[lineNum - 1].BlockEnded)
				{
					curParentIndex = parsedLines[lineNum - 1].ParentIndex;
				}
				else
				{
					curParentIndex = lineNum - 1;
				}
				break;
			}
		}
		LineType lineType = ((curParentIndex != -1) ? parsedLines[curParentIndex].Type : LineType.None);
		switch (li.Type)
		{
		case LineType.None:
		case LineType.EmptyLine:
		case LineType.Comment:
		case LineType.MLCommentPart:
		case LineType.End:
		case LineType.Program:
		case LineType.Member:
		case LineType.Include:
		case LineType.Pragma:
		case LineType.ElsIf:
		case LineType.Else:
		case LineType.Of:
		case LineType.OrOf:
		case LineType.Catch:
		case LineType.While:
		case LineType.Until:
		case LineType.Section:
		case LineType.Namespace:
		case LineType.Using:
			li.ParentIndex = curParentIndex;
			break;
		case LineType.Procedure:
		case LineType.Function:
		case LineType.Routine:
		case LineType.Property:
		case LineType.Indexer:
			switch (lineType)
			{
			case LineType.Map:
			case LineType.Class:
			case LineType.Interface:
			case LineType.Module:
				li.ParentIndex = curParentIndex;
				break;
			default:
				if (!IsWin)
				{
					if (lineType == LineType.Inline)
					{
						li.ParentIndex = curParentIndex;
					}
					else if (curParentIndex != -1 && parsedLines[curParentIndex].ParentIndex != -1 && parsedLines[parsedLines[curParentIndex].ParentIndex].Type == LineType.Inline)
					{
						li.ParentIndex = parsedLines[curParentIndex].ParentIndex;
					}
				}
				if (li.ParentIndex == -1)
				{
					int num = lineNum - 1;
					LineInfo lineInfo = ((num >= 0) ? parsedLines[num] : null);
					bool flag = false;
					while (lineInfo != null)
					{
						switch (lineInfo.Type)
						{
						case LineType.EmptyLine:
						case LineType.Comment:
						case LineType.MLCommentPart:
							lineInfo.ParentIndex = -1;
							break;
						default:
							flag = true;
							break;
						}
						if (flag)
						{
							break;
						}
						num--;
						lineInfo = ((num >= 0) ? parsedLines[num] : null);
					}
				}
				if (li.ParentIndex != -1 && curParentIndex == -1)
				{
					li.ParentIndex = -1;
				}
				curParentIndex = lineNum;
				break;
			}
			break;
		case LineType.Getter:
		case LineType.Setter:
			if (lineType == LineType.Inline)
			{
				li.ParentIndex = curParentIndex;
			}
			else
			{
				int parentIndex = curParentIndex;
				while (parentIndex != -1 && parsedLines[parentIndex].Type != LineType.Inline)
				{
					parentIndex = parsedLines[parentIndex].ParentIndex;
				}
				if (parentIndex != -1)
				{
					li.ParentIndex = parentIndex;
				}
				else
				{
					li.ParentIndex = curParentIndex;
				}
			}
			curParentIndex = lineNum;
			break;
		default:
			li.ParentIndex = curParentIndex;
			curParentIndex = lineNum;
			break;
		}
		if (curParentIndex == -1 || !li.BlockEnded)
		{
			return;
		}
		int num2 = curParentIndex;
		bool flag2 = false;
		while (num2 != -1)
		{
			if (BlockDemandsEnd(parsedLines[num2].Type))
			{
				if (num2 != lineNum)
				{
					li.ParentIndex = num2;
				}
				flag2 = true;
				break;
			}
			num2 = ((num2 < parsedLines[num2].ParentIndex) ? parsedLines[num2].ParentIndex : (-1));
		}
		if (flag2)
		{
			curParentIndex = ((num2 == -1) ? (-1) : parsedLines[num2].ParentIndex);
		}
	}

	protected static bool BlockDemandsEnd(LineType type)
	{
		switch (type)
		{
		case LineType.None:
		case LineType.Comment:
		case LineType.MLCommentPart:
		case LineType.End:
		case LineType.Program:
		case LineType.Member:
		case LineType.Include:
		case LineType.Pragma:
		case LineType.Code:
		case LineType.Data:
		case LineType.Procedure:
		case LineType.Function:
		case LineType.Routine:
		case LineType.Finally:
		case LineType.ElsIf:
		case LineType.Else:
		case LineType.Of:
		case LineType.OrOf:
		case LineType.Catch:
		case LineType.While:
		case LineType.Until:
		case LineType.Section:
		case LineType.Namespace:
		case LineType.Using:
		case LineType.Getter:
		case LineType.Setter:
		case LineType.Property:
		case LineType.Indexer:
			return false;
		default:
			return true;
		}
	}

	protected bool IsInCode(LineInfo li)
	{
		if (li.ParentIndex == -1 || li.Type == LineType.Data)
		{
			return false;
		}
		switch (parsedLines[li.ParentIndex].Type)
		{
		case LineType.Code:
		case LineType.Routine:
		case LineType.Accept:
		case LineType.Begin:
		case LineType.Try:
		case LineType.Finally:
		case LineType.Case:
		case LineType.If:
		case LineType.ElsIf:
		case LineType.Else:
		case LineType.Loop:
		case LineType.Of:
		case LineType.OrOf:
		case LineType.Execute:
		case LineType.Catch:
		case LineType.While:
		case LineType.Until:
		case LineType.Foreach:
		case LineType.Checked:
		case LineType.Unchecked:
		case LineType.Synclock:
			return true;
		default:
			return false;
		}
	}

	protected abstract void SetLineType(LineInfo li);

	protected abstract bool IsHardReservedKeyword(string keyword);

	protected static bool MustHaveLabel(LineType type)
	{
		switch (type)
		{
		case LineType.Procedure:
		case LineType.Function:
		case LineType.Window:
		case LineType.Application:
		case LineType.Class:
		case LineType.File:
		case LineType.View:
		case LineType.Queue:
		case LineType.Report:
		case LineType.Interface:
		case LineType.Routine:
		case LineType.Detail:
		case LineType.Break:
		case LineType.Property:
		case LineType.Struct:
		case LineType.Enum:
			return true;
		default:
			return false;
		}
	}

	protected LineType ParseKeyword(string keyword)
	{
		if (string.IsNullOrEmpty(keyword))
		{
			return LineType.None;
		}
		if (strToKeywordsEnum.ContainsKey(keyword))
		{
			LineType lineType = strToKeywordsEnum[keyword];
			int num = (int)lineType;
			if (IsWin)
			{
				if (num >= 200 && num <= 299)
				{
					lineType = LineType.None;
				}
			}
			else if (num >= 100 && num <= 199)
			{
				lineType = LineType.None;
			}
			return lineType;
		}
		return LineType.None;
	}

	protected string ReplaceStringsAndComments(string initialString)
	{
		string text = initialString;
		Match match = replaceStrings.Match(text);
		while (match.Success)
		{
			text = text.Remove(match.Index + 1, match.Length - 2).Insert(match.Index + 1, new string('-', match.Length - 2));
			match = replaceStrings.Match(text, match.Index + match.Length);
		}
		match = replaceNumericPicture.Match(text);
		while (match.Success)
		{
			text = text.Remove(match.Index + 1, match.Length - 2).Insert(match.Index + 1, new string('-', match.Length - 2));
			match = replaceNumericPicture.Match(text, match.Index + match.Length);
		}
		match = replacePatternPicture1.Match(text);
		while (match.Success)
		{
			text = text.Remove(match.Index + 1, match.Length - 2).Insert(match.Index + 1, new string('-', match.Length - 2));
			match = replacePatternPicture1.Match(text, match.Index + match.Length);
		}
		match = replacePatternPicture2.Match(text);
		while (match.Success)
		{
			text = text.Remove(match.Index + 1, match.Length - 2).Insert(match.Index + 1, new string('-', match.Length - 2));
			match = replacePatternPicture2.Match(text, match.Index + match.Length);
		}
		match = replaceKeyPicture1.Match(text);
		while (match.Success)
		{
			text = text.Remove(match.Index + 1, match.Length - 2).Insert(match.Index + 1, new string('-', match.Length - 2));
			match = replaceKeyPicture1.Match(text, match.Index + match.Length);
		}
		match = replaceKeyPicture2.Match(text);
		while (match.Success)
		{
			text = text.Remove(match.Index + 1, match.Length - 2).Insert(match.Index + 1, new string('-', match.Length - 2));
			match = replaceKeyPicture2.Match(text, match.Index + match.Length);
		}
		if (SupportsMLComments)
		{
			match = replaceMLComment.Match(text);
			while (match.Success)
			{
				text = text.Remove(match.Index, match.Length).Insert(match.Index, new string(' ', match.Length));
				match = replaceMLComment.Match(text, match.Index + match.Length);
			}
			Match match2 = replaceSLComment2.Match(text);
			if (match2.Success)
			{
				string value = match2.Value;
				text = ((value.Length <= 0 || value[0] == '!') ? text.Substring(0, text.Length - value.Length).TrimEnd() : text.Substring(0, text.Length - value.Length + 1).TrimEnd());
			}
		}
		else
		{
			text = replaceSLComment1.Replace(text, "").TrimEnd();
		}
		return text;
	}

	private bool NeedLineContinue(LineInfo li)
	{
		int charIndex;
		char lastMeaningChar = li.GetLastMeaningChar(out charIndex);
		if (!char.IsLetterOrDigit(lastMeaningChar))
		{
			return CharsForValidLineEnd.IndexOf(lastMeaningChar) == -1;
		}
		return false;
	}

	private bool UnclosedBrackets(int lineNum)
	{
		int num = 0;
		while (lineNum >= 0)
		{
			LineInfo lineInfo = parsedLines[lineNum];
			if (lineInfo.ParentIndex == -1 && !lineInfo.LineContinued)
			{
				break;
			}
			num += lineInfo.BracketsCounter;
			lineNum--;
		}
		return num > 0;
	}

	private static bool IsInOMIT(string fileName, int lineNr)
	{
		if (fileName == null)
		{
			return false;
		}
		ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(fileName);
		if (parseInformationIfExist != null && parseInformationIfExist.MostRecentCompilationUnit is ClaCompilationUnit)
		{
			ClaCompilationUnit claCompilationUnit = (ClaCompilationUnit)(object)parseInformationIfExist.MostRecentCompilationUnit;
			foreach (ClaDomOmitRegion omitRegion in claCompilationUnit.OmitRegions)
			{
				if (omitRegion.Omitted && omitRegion.Region.IsInside(lineNr, 1) && omitRegion.Region.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual string AutoIndentLineInternal(IDocument document, int lineNumber, string lineText)
	{
		if (lineNumber != 0)
		{
			return GetIndentationInternal(document, lineNumber - 1) + lineText.TrimStart();
		}
		return lineText;
	}

	private static string GetIndentationInternal(IDocument document, int lineNumber)
	{
		if (lineNumber < 0 || lineNumber > document.TotalNumberOfLines)
		{
			throw new ArgumentOutOfRangeException("lineNumber");
		}
		string lineAsString = TextUtilities.GetLineAsString(document, lineNumber);
		StringBuilder stringBuilder = new StringBuilder();
		string text = lineAsString;
		foreach (char c in text)
		{
			if (!char.IsWhiteSpace(c))
			{
				break;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	protected virtual string SmartIndentLineInternal(string lineText, int lineNr)
	{
		if (lineNr < 0 || lineNr >= parsedLines.Count)
		{
			return lineText;
		}
		LineInfo lineInfo = parsedLines[lineNr];
		string text = lineText.Substring(0, lineInfo.KeywordIndex).TrimEnd();
		if (!string.IsNullOrEmpty(lineInfo.LabelText))
		{
			text = text.TrimStart();
		}
		string text2 = lineText.Substring(lineInfo.KeywordIndex);
		if (lineInfo.KeywordIndex == 0)
		{
			if (SupportsMLComments)
			{
				int num = lineInfo.ProcessedLineText.Length - lineInfo.ProcessedLineText.TrimStart().Length;
				text = lineText.Substring(0, num).TrimEnd();
				text2 = lineText.Substring(num);
			}
			else
			{
				text2 = text2.TrimStart();
			}
		}
		string result = lineText;
		switch (lineInfo.Type)
		{
		case LineType.Comment:
			if (Options.IndentComments)
			{
				text = string.Empty;
				text2 = lineText.TrimStart();
				if (!Options.DontIndentCommentsOnFirstCol || text2.Length != lineText.Length)
				{
					int desiredColumn6 = ((lineInfo.ParentIndex != -1) ? MakeIndentFromParent(lineInfo) : DefaultIndentColumn);
					result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn6);
				}
			}
			break;
		case LineType.MLCommentPart:
			if (Options.IndentComments)
			{
				text = string.Empty;
				text2 = lineText.TrimStart();
				int desiredColumn9 = ((lineInfo.ParentIndex != -1) ? MakeIndentFromParent(lineInfo) : DefaultIndentColumn);
				result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn9);
			}
			break;
		case LineType.Program:
		case LineType.Member:
		{
			int desiredColumn10 = (Options.SetProgramToPreferredCol ? Options.PreferredColumn : DefaultIndentColumn);
			result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn10);
			break;
		}
		case LineType.Namespace:
		{
			int desiredColumn11 = (Options.SetNamespaceToPreferredCol ? Options.PreferredColumn : DefaultIndentColumn);
			result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn11);
			break;
		}
		case LineType.Map:
		{
			int desiredColumn3 = (Options.SetMapToPreferredCol ? Options.PreferredColumn : DefaultIndentColumn);
			result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn3);
			break;
		}
		case LineType.Using:
		{
			int desiredColumn8 = (Options.SetUsingToPreferredCol ? Options.PreferredColumn : DefaultIndentColumn);
			result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn8);
			break;
		}
		case LineType.Pragma:
		case LineType.Section:
		{
			int desiredColumn4 = (Options.SetPragmaToPreferredCol ? Options.PreferredColumn : DefaultIndentColumn);
			result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn4);
			break;
		}
		case LineType.Code:
		case LineType.Data:
		case LineType.Inline:
		{
			LineType lineType = LineType.None;
			LineType lineType2 = LineType.None;
			if (lineInfo.ParentIndex != -1)
			{
				lineType = parsedLines[lineInfo.ParentIndex].Type;
				if (parsedLines[lineInfo.ParentIndex].ParentIndex != -1)
				{
					lineType2 = parsedLines[parsedLines[lineInfo.ParentIndex].ParentIndex].Type;
				}
			}
			if (lineType == LineType.Inline)
			{
				int num4 = KeywordColumnFromIndex(parsedLines[lineInfo.ParentIndex].ProcessedLineText, parsedLines[lineInfo.ParentIndex].KeywordIndex);
				result = IndentToDesiredColumn(lineInfo, text, text2, num4 + Options.IndentSize + 1);
			}
			else if (lineType2 == LineType.Inline)
			{
				int num5 = KeywordColumnFromIndex(parsedLines[parsedLines[lineInfo.ParentIndex].ParentIndex].ProcessedLineText, parsedLines[parsedLines[lineInfo.ParentIndex].ParentIndex].KeywordIndex);
				result = IndentToDesiredColumn(lineInfo, text, text2, num5 + Options.IndentSize + 1);
			}
			else
			{
				result = IndentToDesiredColumn(lineInfo, text, text2, DefaultIndentColumn);
			}
			break;
		}
		case LineType.Getter:
		case LineType.Setter:
			if (lineInfo.ParentIndex != -1)
			{
				int num9 = KeywordColumnFromIndex(parsedLines[lineInfo.ParentIndex].ProcessedLineText, parsedLines[lineInfo.ParentIndex].KeywordIndex);
				result = IndentToDesiredColumn(lineInfo, text, text2, num9 + Options.IndentSize + 1);
			}
			else
			{
				result = IndentToDesiredColumn(lineInfo, text, text2, DefaultIndentColumn);
			}
			break;
		case LineType.Procedure:
		case LineType.Function:
		case LineType.Routine:
		case LineType.Property:
		case LineType.Indexer:
			if (lineInfo.ParentIndex == -1)
			{
				result = IndentToDesiredColumn(lineInfo, text, text2, Options.PreferredColumn);
			}
			else if (parsedLines[lineInfo.ParentIndex].Type == LineType.Inline)
			{
				int desiredColumn5 = Options.PreferredColumn;
				if (parsedLines[lineInfo.ParentIndex].ParentIndex != -1)
				{
					desiredColumn5 = KeywordColumnFromIndex(parsedLines[parsedLines[lineInfo.ParentIndex].ParentIndex].ProcessedLineText, parsedLines[parsedLines[lineInfo.ParentIndex].ParentIndex].KeywordIndex);
				}
				result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn5);
			}
			else
			{
				int num6 = KeywordColumnFromIndex(parsedLines[lineInfo.ParentIndex].ProcessedLineText, parsedLines[lineInfo.ParentIndex].KeywordIndex);
				result = IndentToDesiredColumn(lineInfo, text, text2, num6 + Options.IndentSize + 1);
			}
			break;
		case LineType.Window:
		case LineType.Application:
		case LineType.Class:
		case LineType.File:
		case LineType.View:
		case LineType.Group:
		case LineType.Queue:
		case LineType.Report:
		case LineType.Interface:
		case LineType.Itemize:
		case LineType.Struct:
		case LineType.Enum:
			if (lineInfo.ParentIndex == -1)
			{
				result = IndentToDesiredColumn(lineInfo, text, text2, Options.PreferredColumn);
			}
			else if (parsedLines[lineInfo.ParentIndex].Type == LineType.Inline)
			{
				int desiredColumn7 = Options.PreferredColumn;
				if (parsedLines[lineInfo.ParentIndex].ParentIndex != -1)
				{
					desiredColumn7 = KeywordColumnFromIndex(parsedLines[parsedLines[lineInfo.ParentIndex].ParentIndex].ProcessedLineText, parsedLines[parsedLines[lineInfo.ParentIndex].ParentIndex].KeywordIndex);
				}
				result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn7);
			}
			else
			{
				int num8 = KeywordColumnFromIndex(parsedLines[lineInfo.ParentIndex].ProcessedLineText, parsedLines[lineInfo.ParentIndex].KeywordIndex);
				result = IndentToDesiredColumn(lineInfo, text, text2, num8 + Options.IndentSize + 1);
			}
			break;
		case LineType.Include:
		case LineType.Accept:
		case LineType.Begin:
		case LineType.Try:
		case LineType.Record:
		case LineType.Header:
		case LineType.Detail:
		case LineType.Option:
		case LineType.Break:
		case LineType.Footer:
		case LineType.Join:
		case LineType.Menu:
		case LineType.Menubar:
		case LineType.Toolbar:
		case LineType.Sheet:
		case LineType.Tab:
		case LineType.Ole:
		case LineType.Form:
		case LineType.Case:
		case LineType.If:
		case LineType.Loop:
		case LineType.Execute:
		case LineType.Module:
		case LineType.Foreach:
		case LineType.Checked:
		case LineType.Unchecked:
		case LineType.Synclock:
		{
			if (lineInfo.ParentIndex == -1)
			{
				result = IndentToDesiredColumn(lineInfo, text, text2, DefaultIndentColumn);
				break;
			}
			int desiredColumn2 = MakeIndentFromParent(lineInfo);
			result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn2);
			break;
		}
		case LineType.End:
		case LineType.Finally:
		case LineType.ElsIf:
		case LineType.Else:
		case LineType.Of:
		case LineType.OrOf:
		case LineType.Catch:
		case LineType.While:
		case LineType.Until:
		{
			if (lineInfo.ParentIndex == -1)
			{
				result = IndentToDesiredColumn(lineInfo, text, text2, DefaultIndentColumn);
				break;
			}
			if (parsedLines[lineInfo.ParentIndex].Type == LineType.Routine)
			{
				result = IndentToDesiredColumn(lineInfo, text, text2, Options.IndentSize + 1);
				break;
			}
			int num7 = KeywordColumnFromIndex(parsedLines[lineInfo.ParentIndex].ProcessedLineText, parsedLines[lineInfo.ParentIndex].KeywordIndex);
			if ((lineInfo.Type == LineType.Of || lineInfo.Type == LineType.OrOf || lineInfo.Type == LineType.Else) && parsedLines[lineInfo.ParentIndex].Type == LineType.Case && Options.IndentOfFromCase)
			{
				num7 += Options.IndentSize;
			}
			result = IndentToDesiredColumn(lineInfo, text, text2, num7 + 1);
			break;
		}
		case LineType.None:
		{
			if (lineNr > 0 && parsedLines[lineNr - 1].LineContinued)
			{
				text = string.Empty;
				text2 = lineText.TrimStart();
				int num2 = lineNr - 1;
				while (num2 > 0 && parsedLines[num2 - 1].LineContinued && parsedLines[num2].Type == LineType.None)
				{
					num2--;
				}
				int num3 = KeywordColumnFromIndex(parsedLines[num2].ProcessedLineText, parsedLines[num2].KeywordIndex);
				result = IndentToDesiredColumn(lineInfo, text, text2, num3 + Options.ContinuousLineIndentMultiplier * Options.IndentSize + 1);
				break;
			}
			int desiredColumn = 0;
			bool flag = false;
			if (Options.TreatStatementEndsWithColonAsLabel && codeLabelExpr.IsMatch(lineInfo.ProcessedLineText.Trim()) && IsInCode(lineInfo))
			{
				if (SupportsMLComments)
				{
					if (lineText.TrimStart().StartsWith(lineInfo.ProcessedLineText.Trim()))
					{
						text = string.Empty;
						text2 = lineText.TrimStart();
						desiredColumn = 0;
						flag = true;
					}
				}
				else
				{
					text = string.Empty;
					text2 = lineText.TrimStart();
					desiredColumn = 0;
					flag = true;
				}
			}
			if (!flag)
			{
				desiredColumn = ((lineInfo.ParentIndex != -1) ? MakeIndentFromParent(lineInfo) : ((!string.IsNullOrEmpty(lineInfo.LabelText)) ? Options.PreferredColumn : DefaultIndentColumn));
			}
			result = IndentToDesiredColumn(lineInfo, text, text2, desiredColumn);
			break;
		}
		}
		return result;
	}

	private int MakeIndentFromParent(LineInfo curLI)
	{
		int num = KeywordColumnFromIndex(parsedLines[curLI.ParentIndex].ProcessedLineText, parsedLines[curLI.ParentIndex].KeywordIndex) + 1;
		if (parsedLines[curLI.ParentIndex].Type != LineType.Code)
		{
			num = ((parsedLines[curLI.ParentIndex].Type == LineType.Routine) ? (Options.IndentSize + 1) : ((parsedLines[curLI.ParentIndex].Type != LineType.Case) ? (num + Options.IndentSize) : ((!Options.IndentOfFromCase) ? (num + Options.IndentSize) : (num + 2 * Options.IndentSize))));
		}
		else if (Options.IndentStatementsFromCODE)
		{
			num += Options.IndentSize;
		}
		return num;
	}

	protected string IndentToDesiredColumn(LineInfo curLI, string firstPart, string secondPart, int desiredColumn)
	{
		desiredColumn = CalculateRealColumnNumber(firstPart, desiredColumn);
		string text = firstPart;
		int i = firstPart.Length + 1;
		int num = text.Length % Options.IndentSize;
		if (num != 0)
		{
			text = ((firstPart.Length <= 0 && !(Options.IndentString != "\t")) ? (text + Options.IndentString) : (text + new string(' ', Options.IndentSize - num)));
			i += Options.IndentSize - num;
		}
		string text2 = ((firstPart.Length == 0) ? Options.IndentString : new string(' ', Options.IndentSize));
		for (; i < desiredColumn; i += Options.IndentSize)
		{
			text += text2;
		}
		curLI.KeywordIndex = text.Length;
		return curLI.ProcessedLineText = text + secondPart;
	}

	protected int KeywordColumnFromIndex(string lineText, int keywordIndex)
	{
		int num = -1;
		int num2 = 0;
		while (true)
		{
			int num3 = lineText.IndexOf('\t', num + 1);
			if (num3 == -1 || num3 >= keywordIndex)
			{
				break;
			}
			int num4 = (num3 - num) % Options.IndentSize;
			if (num4 != 0)
			{
				num2 += Options.IndentSize - num4;
			}
			num = num3;
		}
		return keywordIndex + num2;
	}

	protected int CalculateRealColumnNumber(string firstPart, int desiredColumn)
	{
		if (string.IsNullOrEmpty(firstPart) && desiredColumn <= 1)
		{
			return desiredColumn;
		}
		int num = desiredColumn - 1;
		if (firstPart.Length >= num)
		{
			int num2 = Options.IndentSize * 2;
			while (firstPart.Length >= num)
			{
				num += num2;
				if (num % Options.IndentSize != 0)
				{
					num += Options.IndentSize - num % Options.IndentSize;
				}
			}
			return num + 1;
		}
		int length = firstPart.Length;
		for (length += Options.IndentSize - length % Options.IndentSize; length < num; length += Options.IndentSize)
		{
		}
		return length + 1;
	}

	protected int IndentNewLine(TextArea textArea, int lineNum)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		if ((int)textArea.Document.TextEditorProperties.IndentStyle == 0)
		{
			return 0;
		}
		if (parsedLines == null || lineNum >= parsedLines.Count || (int)textArea.Document.TextEditorProperties.IndentStyle == 1)
		{
			return ((DefaultFormattingStrategy)this).AutoIndentLine(textArea, lineNum);
		}
		LineSegment lineSegment = textArea.Document.GetLineSegment(lineNum);
		string text = textArea.Document.GetText((ISegment)(object)lineSegment);
		if (text.Trim() != string.Empty)
		{
			return ((DefaultFormattingStrategy)this).SmartIndentLine(textArea, lineNum);
		}
		LineInfo lineInfo = parsedLines[lineNum];
		LineInfo lineInfo2 = parsedLines[lineNum - 1];
		LineInfo lineInfo3 = ((lineInfo.ParentIndex == -1) ? null : parsedLines[lineInfo.ParentIndex]);
		string text2 = string.Empty;
		if (lineInfo2.LineContinued)
		{
			int num = KeywordColumnFromIndex(lineInfo2.ProcessedLineText, lineInfo2.KeywordIndex);
			text2 = IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, num + Options.ContinuousLineIndentMultiplier * Options.IndentSize + 1);
		}
		else
		{
			if (lineInfo3 == null)
			{
				LineInfo lineInfo4 = lineInfo2;
				int num2 = lineNum - 1;
				while (lineInfo4 != null && (lineInfo4.Type == LineType.Comment || lineInfo4.Type == LineType.EmptyLine || lineInfo4.Type == LineType.MLCommentPart))
				{
					num2--;
					lineInfo4 = ((num2 < 0) ? null : parsedLines[num2]);
				}
				if (lineInfo4 != null)
				{
					lineInfo2 = lineInfo4;
				}
				if (num2 > 0 && lineInfo2.Type == LineType.None && parsedLines[num2 - 1].LineContinued)
				{
					for (num2--; num2 >= 0; num2--)
					{
						lineInfo4 = parsedLines[num2];
						if (lineInfo4.Type != LineType.None || !lineInfo4.LineContinued)
						{
							break;
						}
					}
					lineInfo2 = lineInfo4;
				}
			}
			LineInfo lineInfo5 = lineInfo3 ?? lineInfo2;
			switch (lineInfo5.Type)
			{
			case LineType.EmptyLine:
			case LineType.Comment:
				if (lineInfo5.ParentIndex != -1)
				{
					int num = KeywordColumnFromIndex(parsedLines[lineInfo5.ParentIndex].ProcessedLineText, parsedLines[lineInfo5.ParentIndex].KeywordIndex);
					text2 = IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, num + Options.IndentSize + 1);
				}
				else
				{
					text2 = AutoIndentLineInternal(textArea.Document, lineNum, string.Empty);
				}
				break;
			case LineType.MLCommentPart:
				text2 = AutoIndentLineInternal(textArea.Document, lineNum, string.Empty);
				break;
			case LineType.None:
			case LineType.Program:
			case LineType.Member:
			case LineType.Include:
			case LineType.Pragma:
			case LineType.Section:
			case LineType.Namespace:
			case LineType.Using:
				if (lineInfo5.ParentIndex != -1)
				{
					int num = KeywordColumnFromIndex(lineInfo5.ProcessedLineText, lineInfo5.KeywordIndex);
					text2 = IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, num + 1);
				}
				else
				{
					text2 = string.Empty;
				}
				break;
			case LineType.Map:
			case LineType.Data:
			case LineType.Procedure:
			case LineType.Function:
			case LineType.Class:
			case LineType.File:
			case LineType.View:
			case LineType.Group:
			case LineType.Queue:
			case LineType.Interface:
			case LineType.Record:
			case LineType.Module:
			case LineType.Itemize:
			case LineType.Inline:
			case LineType.Getter:
			case LineType.Setter:
			case LineType.Property:
			case LineType.Indexer:
			case LineType.Struct:
			case LineType.Enum:
				text2 = string.Empty;
				break;
			case LineType.Code:
			{
				int num = KeywordColumnFromIndex(lineInfo5.ProcessedLineText, lineInfo5.KeywordIndex);
				text2 = ((!Options.IndentStatementsFromCODE) ? IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, num + 1) : IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, num + Options.IndentSize + 1));
				break;
			}
			case LineType.Routine:
				text2 = IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, Options.IndentSize + 1);
				break;
			case LineType.Window:
			case LineType.Application:
			case LineType.Report:
			case LineType.Accept:
			case LineType.Begin:
			case LineType.Try:
			case LineType.Header:
			case LineType.Detail:
			case LineType.Option:
			case LineType.Break:
			case LineType.Footer:
			case LineType.Join:
			case LineType.Menu:
			case LineType.Menubar:
			case LineType.Toolbar:
			case LineType.Sheet:
			case LineType.Tab:
			case LineType.Ole:
			case LineType.Form:
			case LineType.Case:
			case LineType.If:
			case LineType.Loop:
			case LineType.Execute:
			case LineType.Foreach:
			case LineType.Checked:
			case LineType.Unchecked:
			case LineType.Synclock:
			{
				int num4 = ((!lineInfo5.BlockEnded) ? ((lineInfo5.Type != LineType.Case) ? Options.IndentSize : ((lineNum - 1 != lineInfo.ParentIndex) ? (Options.IndentOfFromCase ? (2 * Options.IndentSize) : Options.IndentSize) : (Options.IndentOfFromCase ? Options.IndentSize : 0))) : 0);
				int num = KeywordColumnFromIndex(lineInfo5.ProcessedLineText, lineInfo5.KeywordIndex);
				text2 = IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, num + num4 + 1);
				break;
			}
			case LineType.Finally:
			case LineType.ElsIf:
			case LineType.Else:
			case LineType.Of:
			case LineType.OrOf:
			case LineType.Catch:
			{
				int num3 = Options.IndentSize;
				if (lineInfo5.BlockEnded)
				{
					num3 = 0;
				}
				int num;
				if (lineInfo5.ParentIndex != -1)
				{
					if ((lineInfo5.Type == LineType.Of || lineInfo5.Type == LineType.OrOf || lineInfo5.Type == LineType.Else) && parsedLines[lineInfo5.ParentIndex].Type == LineType.Case && Options.IndentOfFromCase)
					{
						num3 = 2 * Options.IndentSize;
					}
					num = KeywordColumnFromIndex(parsedLines[lineInfo5.ParentIndex].ProcessedLineText, parsedLines[lineInfo5.ParentIndex].KeywordIndex);
				}
				else
				{
					num = KeywordColumnFromIndex(lineInfo5.ProcessedLineText, lineInfo5.KeywordIndex);
				}
				text2 = IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, num + num3 + 1);
				break;
			}
			case LineType.End:
			case LineType.While:
			case LineType.Until:
				if (lineInfo5.ParentIndex != -1)
				{
					if (parsedLines[lineInfo5.ParentIndex].ParentIndex == -1)
					{
						text2 = string.Empty;
						break;
					}
					int num = KeywordColumnFromIndex(parsedLines[lineInfo5.ParentIndex].ProcessedLineText, parsedLines[lineInfo5.ParentIndex].KeywordIndex);
					text2 = IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, num + 1);
				}
				else
				{
					int num = KeywordColumnFromIndex(lineInfo5.ProcessedLineText, lineInfo5.KeywordIndex);
					text2 = IndentToDesiredColumn(lineInfo, string.Empty, string.Empty, num + 1);
				}
				break;
			}
		}
		if (text2 != text)
		{
			monitorDocChanges = false;
			textArea.Document.Replace(lineSegment.Offset, lineSegment.Length, text2);
			monitorDocChanges = true;
		}
		return text2.Length;
	}

	protected override int AutoIndentLine(TextArea textArea, int lineNumber)
	{
		string text = ((lineNumber != 0) ? ((DefaultFormattingStrategy)this).GetIndentation(textArea, lineNumber - 1) : string.Empty);
		if (text.Length > 0)
		{
			LineSegment lineSegment = textArea.Document.GetLineSegment(lineNumber);
			string text2 = textArea.Document.GetText((ISegment)(object)lineSegment);
			string text3 = AutoIndentLineInternal(textArea.Document, lineNumber, text2);
			if (text2 != text3)
			{
				monitorDocChanges = false;
				textArea.Document.Replace(lineSegment.Offset, lineSegment.Length, text3);
				monitorDocChanges = true;
			}
		}
		return text.Length;
	}

	protected override int SmartIndentLine(TextArea textArea, int lineNr)
	{
		if (lineNr < 0 || lineNr >= parsedLines.Count)
		{
			return ((DefaultFormattingStrategy)this).AutoIndentLine(textArea, lineNr);
		}
		LineSegment lineSegment = textArea.Document.GetLineSegment(lineNr);
		string text = textArea.Document.GetText((ISegment)(object)lineSegment);
		string text2 = SmartIndentLineInternal(text, lineNr);
		if (text2 != text)
		{
			monitorDocChanges = false;
			textArea.Document.Replace(lineSegment.Offset, lineSegment.Length, text2);
			monitorDocChanges = true;
			return text2.Length - text2.TrimStart().Length;
		}
		return 0;
	}

	public override void FormatLine(TextArea textArea, int lineNr, int cursorOffset, char ch)
	{
		textArea.Document.UndoStack.StartUndoGroup();
		try
		{
			FormatLineInternal(textArea, lineNr, cursorOffset, ch);
		}
		finally
		{
			textArea.Document.UndoStack.EndUndoGroup();
		}
	}

	private void FormatLineInternal(TextArea textArea, int lineNr, int cursorOffset, char ch)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Invalid comparison between Unknown and I4
		char c = ch;
		if (c != '\n')
		{
			return;
		}
		if (AutoInsertLineContinuation && parsedLines != null && lineNr >= 1 && lineNr < parsedLines.Count && !IsInOMIT(((TextEditorControlBase)textArea.MotherTextEditorControl).FileName, lineNr - 1))
		{
			LineInfo lineInfo = parsedLines[lineNr - 1];
			if (!lineInfo.LineContinued)
			{
				bool flag = false;
				if (lineInfo.ProcessedLineText.Trim() == string.Empty)
				{
					flag = true;
					if (lineNr - 2 >= 0 && parsedLines[lineNr - 2].LineContinued)
					{
						flag = false;
					}
				}
				if (!flag && NeedLineContinuation(lineNr - 1, lineInfo, textArea))
				{
					InsertLineContinuation(lineNr - 1, lineInfo, textArea);
				}
			}
		}
		if ((int)textArea.Document.TextEditorProperties.IndentStyle == 2 && lineNr > 0 && parsedLines != null)
		{
			bool flag2 = false;
			if (Options.FormatBlockAfterEnd)
			{
				LineInfo lineInfo2 = parsedLines[lineNr - 1];
				if (lineInfo2.BlockEnded && !BlockDemandsEnd(lineInfo2.Type) && lineInfo2.ParentIndex != -1 && (IsInCode(lineInfo2) || !Options.FormatBlockAfterEndOnlyInCode))
				{
					flag2 = true;
					((DefaultFormattingStrategy)this).IndentLines(textArea, lineInfo2.ParentIndex, lineNr - 1);
				}
			}
			if (!flag2 && Options.EnableEnteredLineFormatting)
			{
				((DefaultFormattingStrategy)this).SmartIndentLine(textArea, lineNr - 1);
			}
		}
		textArea.Caret.Column = IndentNewLine(textArea, lineNr);
	}

	protected virtual bool NeedLineContinuation(int lineNr, LineInfo li, TextArea textArea)
	{
		if (UnclosedBrackets(lineNr) || NeedLineContinue(li))
		{
			return true;
		}
		return false;
	}

	private static void InsertLineContinuation(int lineNr, LineInfo li, TextArea textArea)
	{
		li.GetLastMeaningChar(out var charIndex);
		LineSegment lineSegment = textArea.Document.GetLineSegment(lineNr);
		textArea.Document.Insert(lineSegment.Offset + charIndex + 1, " |");
	}

	public override void IndentLines(TextArea textArea, int begin, int end)
	{
		IndentLines(textArea.Document, begin, end);
	}

	public void IndentLines(IDocument document, int begin, int end)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		IndentStyle indentStyle = document.TextEditorProperties.IndentStyle;
		if ((int)indentStyle == 0)
		{
			return;
		}
		List<AffectedLinesBlock> list = new List<AffectedLinesBlock>();
		for (int i = begin; i <= end; i++)
		{
			LineSegment lineSegment = document.GetLineSegment(i);
			string text = document.GetText((ISegment)(object)lineSegment);
			string text2 = (((int)indentStyle != 1) ? SmartIndentLineInternal(text, i) : AutoIndentLineInternal(document, i, text));
			if (!(text2 != text))
			{
				continue;
			}
			if (list.Count == 0)
			{
				list.Add(new AffectedLinesBlock(i, text2));
				continue;
			}
			AffectedLinesBlock affectedLinesBlock = list[list.Count - 1];
			if (affectedLinesBlock.EndLine == i - 1)
			{
				affectedLinesBlock.AddLineText(text2);
			}
			else
			{
				list.Add(new AffectedLinesBlock(i, text2));
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		document.UndoStack.StartUndoGroup();
		try
		{
			List<char> list2 = new List<char>();
			foreach (AffectedLinesBlock item in list)
			{
				list2.Clear();
				for (int j = 0; j < item.TextLines.Count; j++)
				{
					string text3 = item.TextLines[j];
					if (j != 0)
					{
						LineSegment lineSegment2 = document.GetLineSegment(item.BeginLine + j - 1);
						string text4 = document.GetText(lineSegment2.Offset + lineSegment2.Length, lineSegment2.DelimiterLength);
						list2.AddRange(text4.ToCharArray());
					}
					list2.AddRange(text3.ToCharArray());
				}
				LineSegment lineSegment3 = document.GetLineSegment(item.BeginLine);
				LineSegment lineSegment4 = document.GetLineSegment(item.EndLine);
				monitorDocChanges = false;
				document.Replace(lineSegment3.Offset, lineSegment4.Offset + lineSegment4.Length - lineSegment3.Offset, new string(list2.ToArray()));
				monitorDocChanges = true;
			}
		}
		finally
		{
			document.UndoStack.EndUndoGroup();
		}
	}

	public override int SearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket)
	{
		int lineNumberForOffset = document.GetLineNumberForOffset(offset);
		int lineOffset = offset - document.GetLineSegment(lineNumberForOffset).Offset;
		if (GetStartType(document, lineNumberForOffset, lineOffset) != 0)
		{
			return -1;
		}
		int num = 1;
		while (offset >= 0)
		{
			char charAt = document.GetCharAt(offset);
			if (charAt == closingBracket)
			{
				lineNumberForOffset = document.GetLineNumberForOffset(offset);
				if (GetStartType(document, lineNumberForOffset, offset - document.GetLineSegment(lineNumberForOffset).Offset) == 0)
				{
					num++;
				}
			}
			else if (charAt == openBracket)
			{
				lineNumberForOffset = document.GetLineNumberForOffset(offset);
				if (GetStartType(document, lineNumberForOffset, offset - document.GetLineSegment(lineNumberForOffset).Offset) == 0)
				{
					num--;
					if (num == 0)
					{
						return offset;
					}
				}
			}
			offset--;
		}
		return -1;
	}

	public override int SearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket)
	{
		int lineNumberForOffset = document.GetLineNumberForOffset(offset);
		int lineOffset = offset - document.GetLineSegment(lineNumberForOffset).Offset;
		if (GetStartType(document, lineNumberForOffset, lineOffset) != 0)
		{
			return -1;
		}
		int num = 1;
		while (offset < document.TextLength)
		{
			char charAt = document.GetCharAt(offset);
			if (charAt == openBracket)
			{
				lineNumberForOffset = document.GetLineNumberForOffset(offset);
				if (GetStartType(document, lineNumberForOffset, offset - document.GetLineSegment(lineNumberForOffset).Offset) == 0)
				{
					num++;
				}
			}
			else if (charAt == closingBracket)
			{
				lineNumberForOffset = document.GetLineNumberForOffset(offset);
				if (GetStartType(document, lineNumberForOffset, offset - document.GetLineSegment(lineNumberForOffset).Offset) == 0)
				{
					num--;
					if (num == 0)
					{
						return offset;
					}
				}
			}
			offset++;
		}
		return -1;
	}

	protected int GetStartType(IDocument document, int lineNum, int lineOffset)
	{
		string text = document.GetText((ISegment)(object)document.GetLineSegment(lineNum));
		string text2;
		if (parsedLines != null)
		{
			switch (parsedLines[lineNum].Type)
			{
			case LineType.EmptyLine:
			case LineType.Comment:
			case LineType.MLCommentPart:
				return 1;
			}
			text2 = parsedLines[lineNum].ProcessedLineText;
		}
		else
		{
			text2 = ReplaceStringsAndComments(text);
			int num = text2.IndexOf("~!");
			if (num != -1)
			{
				text2 = new string(' ', num + 2) + text2.Substring(num + 2);
			}
			num = text2.IndexOf("!~");
			if (num != -1)
			{
				text2 = text2.Substring(0, num).TrimEnd();
			}
		}
		if (lineOffset >= text2.Length)
		{
			return 1;
		}
		if (text2[lineOffset] != text[lineOffset])
		{
			if (text2[lineOffset] == '-')
			{
				return 2;
			}
			return 1;
		}
		return 0;
	}

	public string GetExpressionBeforePos(int line, int column)
	{
		if (Disposed || parsedLines == null || line < 0 || line >= parsedLines.Count)
		{
			return null;
		}
		if (column < 0)
		{
			column = 0;
		}
		LineInfo lineInfo = parsedLines[line];
		if (lineInfo != null && lineInfo.ProcessedLineText.Length < column)
		{
			column = lineInfo.ProcessedLineText.Length;
		}
		if (line == 0)
		{
			return lineInfo.ProcessedLineText.Substring(0, column);
		}
		int notEmptyNotCommentPrevLineIndex = GetNotEmptyNotCommentPrevLineIndex(line);
		while (notEmptyNotCommentPrevLineIndex != -1 && parsedLines[notEmptyNotCommentPrevLineIndex].LineContinued)
		{
			notEmptyNotCommentPrevLineIndex = GetNotEmptyNotCommentPrevLineIndex(notEmptyNotCommentPrevLineIndex);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = notEmptyNotCommentPrevLineIndex + 1; i < line; i++)
		{
			stringBuilder.Append(parsedLines[i].ProcessedLineText);
		}
		stringBuilder.Append(lineInfo.ProcessedLineText.Substring(0, column));
		return stringBuilder.ToString();
	}

	private int GetNotEmptyNotCommentPrevLineIndex(int lineNum)
	{
		if (lineNum <= 0)
		{
			return -1;
		}
		int num = lineNum - 1;
		LineInfo lineInfo = parsedLines[num];
		while (num >= 0 && (lineInfo.Type == LineType.Comment || lineInfo.Type == LineType.EmptyLine || lineInfo.Type == LineType.MLCommentPart))
		{
			num--;
			lineInfo = ((num < 0) ? null : parsedLines[num]);
		}
		return num;
	}
}
