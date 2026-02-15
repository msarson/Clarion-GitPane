using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Generator;

public class ClarionTemplateFolding : IFoldingStrategy, IDisposable
{
	private const string functionPattern = "^\\s*\\#(?<functionType>(TEMPLATE|APPLICATION|UTILITY|CONTROL|SYSTEM|PROGRAM|MODULE|PROCEDURE|CODE|EXTENSION))\\s*(\\(\\s*(?<functionName>\\w+)?(\\s*((,)?\\s*(?<functionDescription>'?[^']*'?))*)\\s*\\)*.|\\z)";

	private const string groupPattern = "^\\s*\\#GROUP\\s*\\(\\s*%(?<groupName>\\w*)(?<groupDescription>.*)";

	private const string keyWordOpenFoldingPattern = "^\\s*\\#(?<keyOpenName>(AT|ATSTART|IF|CASE|LOOP|FOR|BOXED|BUTTON|ENABLE|SHEET|TAB|PREPARE|CONTEXT|SECTION|WITH|GLOBALDATA|LOCALDATA|WINDOWS|REPORTS|RESTRICT|SUSPEND))(([,|\\s|\\(])|\\z)";

	private const string keyWordCloseFoldingPattern = "^\\s*\\#(?<keyCloseName>(ENDAT|END|ENDIF|ENDCASE|ENDLOOP|ENDCASE|ENDFOR|ENDBOXED|ENDBUTTON|ENDENABLE|ENDSHEET|ENDTAB|ENDPREPARE|ENDCONTEXT|ENDSECTION|END|ENDGLOBALDATA|ENDLOCALDATA|ENDWINDOWS|ENDREPORTS|ENDRESTRICT|RESUME))\\s*\\z";

	private const string RegionPattern = "^\\s*#!(?<keyRegion>REGION)((\\s*\\z)|(\\s+(?<keyRegionDescription>.*)))";

	private const string endRegionPattern = "(\\s*\\#!(?<keyEndRegion>ENDREGION)\\s*\\z)";

	private static Dictionary<string, List<string>> tokens = new Dictionary<string, List<string>>();

	private List<string> functionTokens = new List<string>();

	private List<ClarionTemplateParsedFunction> _listOfFunctions = new List<ClarionTemplateParsedFunction>();

	private List<KeyValuePair<string, List<ClarionTemplateParsedFunction>>> _listOfFunctionsSorted = new List<KeyValuePair<string, List<ClarionTemplateParsedFunction>>>();

	private ClarionTemplateParsedFunction _firstFunction;

	private static Regex _markersRegex = null;

	private static Regex _isTemplateLineRegex = new Regex("^\\s*#");

	private bool sorted;

	private bool _Disposed;

	public ClarionTemplateParsedFunction FirstFunction => _firstFunction;

	private static Regex MarkersRegex
	{
		get
		{
			if (_markersRegex == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("^\\s*\\#(?<keyOpenName>(AT|ATSTART|IF|CASE|LOOP|FOR|BOXED|BUTTON|ENABLE|SHEET|TAB|PREPARE|CONTEXT|SECTION|WITH|GLOBALDATA|LOCALDATA|WINDOWS|REPORTS|RESTRICT|SUSPEND))(([,|\\s|\\(])|\\z)");
				stringBuilder.Append("|");
				stringBuilder.Append("^\\s*\\#(?<keyCloseName>(ENDAT|END|ENDIF|ENDCASE|ENDLOOP|ENDCASE|ENDFOR|ENDBOXED|ENDBUTTON|ENDENABLE|ENDSHEET|ENDTAB|ENDPREPARE|ENDCONTEXT|ENDSECTION|END|ENDGLOBALDATA|ENDLOCALDATA|ENDWINDOWS|ENDREPORTS|ENDRESTRICT|RESUME))\\s*\\z");
				stringBuilder.Append("|");
				stringBuilder.Append("^\\s*\\#(?<functionType>(TEMPLATE|APPLICATION|UTILITY|CONTROL|SYSTEM|PROGRAM|MODULE|PROCEDURE|CODE|EXTENSION))\\s*(\\(\\s*(?<functionName>\\w+)?(\\s*((,)?\\s*(?<functionDescription>'?[^']*'?))*)\\s*\\)*.|\\z)");
				stringBuilder.Append("|");
				stringBuilder.Append("^\\s*\\#GROUP\\s*\\(\\s*%(?<groupName>\\w*)(?<groupDescription>.*)");
				stringBuilder.Append("|");
				stringBuilder.Append("^\\s*#!(?<keyRegion>REGION)((\\s*\\z)|(\\s+(?<keyRegionDescription>.*)))");
				stringBuilder.Append("|");
				stringBuilder.Append("(\\s*\\#!(?<keyEndRegion>ENDREGION)\\s*\\z)");
				_markersRegex = new Regex(stringBuilder.ToString(), RegexOptions.Singleline);
			}
			return _markersRegex;
		}
	}

	public ClarionTemplateFolding()
	{
		if (tokens.Count == 0)
		{
			AddCodeFoldingTokens("AT", "ENDAT");
			AddCodeFoldingTokens("ATSTART", "ENDAT");
			AddCodeFoldingTokens("IF", "END", "ENDIF");
			AddCodeFoldingTokens("CASE", "END", "ENDCASE");
			AddCodeFoldingTokens("LOOP", "ENDLOOP");
			AddCodeFoldingTokens("FOR", "ENDFOR");
			AddCodeFoldingTokens("BOXED", "ENDBOXED");
			AddCodeFoldingTokens("BUTTON", "ENDBUTTON");
			AddCodeFoldingTokens("ENABLE", "ENDENABLE");
			AddCodeFoldingTokens("SHEET", "ENDSHEET");
			AddCodeFoldingTokens("TAB", "ENDTAB");
			AddCodeFoldingTokens("PREPARE", "ENDPREPARE");
			AddCodeFoldingTokens("CONTEXT", "ENDCONTEXT");
			AddCodeFoldingTokens("SECTION", "ENDSECTION");
			AddCodeFoldingTokens("WITH", "END");
			AddCodeFoldingTokens("GLOBALDATA", "ENDGLOBALDATA");
			AddCodeFoldingTokens("LOCALDATA", "ENDLOCALDATA");
			AddCodeFoldingTokens("WINDOWS", "ENDWINDOWS");
			AddCodeFoldingTokens("REPORTS", "ENDREPORTS");
			AddCodeFoldingTokens("RESTRICT", "ENDRESTRICT");
			AddCodeFoldingTokens("SUSPEND", "RESUME");
			AddCodeFoldingTokens("REGION", "ENDREGION");
		}
	}

	private void AddCodeFoldingTokens(string start, string ending)
	{
		AddCodeFoldingTokens(start, new string[1] { ending.ToUpperInvariant() });
	}

	private void AddCodeFoldingTokens(string start, params string[] ending)
	{
		if (tokens.ContainsKey(start))
		{
			List<string> list = tokens[start];
			foreach (string text in ending)
			{
				list.Add(text.ToUpperInvariant());
			}
			return;
		}
		List<string> list2 = new List<string>();
		foreach (string text2 in ending)
		{
			list2.Add(text2.ToUpperInvariant());
		}
		tokens.Add(start.ToUpperInvariant(), list2);
	}

	public List<ClarionTemplateParsedFunction> ListOfFunctions()
	{
		return _listOfFunctions;
	}

	public IEnumerable<ClarionTemplateParsedFunction> ListOfFunctions(string functionType)
	{
		foreach (ClarionTemplateParsedFunction f in _listOfFunctions)
		{
			if (f.FunctionType == functionType)
			{
				yield return f;
			}
		}
	}

	public List<KeyValuePair<string, List<ClarionTemplateParsedFunction>>> ListOfFunctionsSorted()
	{
		return _listOfFunctionsSorted;
	}

	public List<ClarionTemplateParsedFunction> ListOfFunctionsSorted(string functionType)
	{
		foreach (KeyValuePair<string, List<ClarionTemplateParsedFunction>> item in _listOfFunctionsSorted)
		{
			if (item.Key == functionType)
			{
				return item.Value;
			}
		}
		return new List<ClarionTemplateParsedFunction>();
	}

	protected List<ClarionTemplateParsedFunction> ContainsParsedFunctionType(string functionType)
	{
		foreach (KeyValuePair<string, List<ClarionTemplateParsedFunction>> item in _listOfFunctionsSorted)
		{
			if (item.Key == functionType)
			{
				return item.Value;
			}
		}
		return null;
	}

	protected void AddParsedFunction(ClarionTemplateParsedFunction parsedFunction)
	{
		sorted = false;
		_listOfFunctions.Add(parsedFunction);
		List<ClarionTemplateParsedFunction> list = ContainsParsedFunctionType(parsedFunction.FunctionType);
		if (list != null)
		{
			list.Add(parsedFunction);
			return;
		}
		list = new List<ClarionTemplateParsedFunction>();
		list.Add(parsedFunction);
		_listOfFunctionsSorted.Add(new KeyValuePair<string, List<ClarionTemplateParsedFunction>>(parsedFunction.FunctionType, list));
	}

	protected void ClearFunctionsList()
	{
		_listOfFunctions.Clear();
		foreach (KeyValuePair<string, List<ClarionTemplateParsedFunction>> item in _listOfFunctionsSorted)
		{
			item.Value.Clear();
		}
		_listOfFunctionsSorted.Clear();
	}

	public bool GetIndexInListOfFunctions(int lineNumber, out int functionTypeIndex, out int functionIndex)
	{
		ClarionTemplateParsedFunction newFunction = _listOfFunctions.FindLast((ClarionTemplateParsedFunction a) => lineNumber >= a.LineNumber);
		functionTypeIndex = -1;
		functionIndex = -1;
		if (newFunction != null)
		{
			int num = -1;
			foreach (KeyValuePair<string, List<ClarionTemplateParsedFunction>> item in _listOfFunctionsSorted)
			{
				num++;
				if (item.Key == newFunction.FunctionType)
				{
					functionTypeIndex = num;
					functionIndex = item.Value.FindIndex((ClarionTemplateParsedFunction a) => newFunction.LineNumber == a.LineNumber);
					break;
				}
			}
			return true;
		}
		return false;
	}

	private static bool IsTemplateLine(ref string textLine)
	{
		return _isTemplateLineRegex.IsMatch(textLine);
	}

	public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation)
	{
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Expected O, but got Unknown
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Expected O, but got Unknown
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Expected O, but got Unknown
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Expected O, but got Unknown
		_firstFunction = null;
		List<FoldMarker> list = new List<FoldMarker>();
		Stack<KeyValuePair<int, List<string>>> stack = new Stack<KeyValuePair<int, List<string>>>();
		List<string> list2 = null;
		int num = 0;
		bool flag = false;
		int num2 = 0;
		_listOfFunctions.Clear();
		_listOfFunctionsSorted.Clear();
		Group obj = null;
		Group obj2 = null;
		Group obj3 = null;
		Group obj4 = null;
		Group obj5 = null;
		Group obj6 = null;
		Group obj7 = null;
		Group obj8 = null;
		Group obj9 = null;
		Match match = null;
		string text = null;
		string text2 = "";
		string text3 = "";
		for (int i = 0; i < document.TotalNumberOfLines; i++)
		{
			text = document.GetText((ISegment)(object)document.GetLineSegment(i));
			if (!IsTemplateLine(ref text))
			{
				continue;
			}
			match = MarkersRegex.Match(text.ToUpper());
			if (!match.Success)
			{
				continue;
			}
			obj = match.Groups["functionType"];
			obj2 = match.Groups["functionName"];
			obj3 = match.Groups["functionDescription"];
			obj4 = match.Groups["groupName"];
			obj5 = match.Groups["groupDescription"];
			obj6 = match.Groups["keyOpenName"];
			obj7 = match.Groups["keyCloseName"];
			obj8 = match.Groups["keyRegion"];
			_ = match.Groups["keyRegionDescription"];
			obj9 = match.Groups["keyEndRegion"];
			if (obj.Success)
			{
				if (flag && num2 < i - 1)
				{
					list.Add(new FoldMarker(document, num2, document.GetLineSegment(num2).Length, i - 1, document.GetLineSegment(i - 1).Length));
				}
				if (obj3.Captures.Count < 2)
				{
					text2 = GetOriginalText(ref text, obj2);
					text3 = GetOriginalText(ref text, obj3);
				}
				else
				{
					text2 = GetOriginalText(ref text, obj2);
					text3 = GetOriginalText(ref text, obj3.Captures[0]);
				}
				if (string.IsNullOrEmpty(text2))
				{
					text2 = obj.Value.ToUpperInvariant();
				}
				AddParsedFunction(new ClarionTemplateParsedFunction(i, obj.Value.ToUpperInvariant(), text2, text3));
				num2 = i;
				flag = true;
				list2 = null;
				stack.Clear();
			}
			else if (obj4.Success)
			{
				if (flag && num2 < i - 1)
				{
					list.Add(new FoldMarker(document, num2, document.GetLineSegment(num2).Length, i - 1, document.GetLineSegment(i - 1).Length));
				}
				string text4 = GetOriginalText(ref text, obj5).TrimStart(' ', ',');
				AddParsedFunction(new ClarionTemplateParsedFunction(i, "GROUP", GetOriginalText(ref text, obj4), "(" + text4));
				num2 = i;
				flag = true;
				list2 = null;
				stack.Clear();
			}
			else if (obj8.Success)
			{
				list2 = tokens["REGION"];
				stack.Push(new KeyValuePair<int, List<string>>(i, list2));
			}
			else if (obj9.Success)
			{
				if (list2 != null && list2.Contains("ENDREGION"))
				{
					num = stack.Pop().Key;
					list.Add(new FoldMarker(document, num, document.GetLineSegment(num).Length, i, document.GetLineSegment(i).Length));
					list2 = ((stack.Count <= 0) ? null : stack.Peek().Value);
				}
			}
			else if (obj6.Success)
			{
				list2 = tokens[obj6.Value.ToUpperInvariant()];
				stack.Push(new KeyValuePair<int, List<string>>(i, list2));
			}
			else if (obj7.Success && list2 != null && list2.Contains(obj7.Value.ToUpperInvariant()))
			{
				num = stack.Pop().Key;
				list.Add(new FoldMarker(document, num, document.GetLineSegment(num).Length, i, document.GetLineSegment(i).Length));
				list2 = ((stack.Count <= 0) ? null : stack.Peek().Value);
			}
		}
		if (flag)
		{
			list.Add(new FoldMarker(document, num2, document.GetLineSegment(num2).Length, document.LineSegmentCollection.Count - 1, document.GetLineSegment(document.LineSegmentCollection.Count - 1).Length));
		}
		if (_listOfFunctions.Count > 0)
		{
			_firstFunction = _listOfFunctions[0];
		}
		return list;
	}

	private string GetOriginalText(ref string originalText, Capture capture)
	{
		return originalText.Substring(capture.Index, capture.Length);
	}

	public void SortListOfFunctions()
	{
		if (sorted)
		{
			return;
		}
		if (_listOfFunctionsSorted.Count > 0)
		{
			_firstFunction = _listOfFunctions[0];
			_listOfFunctionsSorted.Sort(_firstFunction);
			foreach (KeyValuePair<string, List<ClarionTemplateParsedFunction>> item in _listOfFunctionsSorted)
			{
				item.Value.Sort(_firstFunction);
			}
		}
		sorted = true;
	}

	public void OnDocumentClosed()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (!_Disposed)
		{
			_Disposed = true;
			functionTokens.Clear();
			_listOfFunctions.Clear();
			_listOfFunctionsSorted.Clear();
			_firstFunction = null;
			functionTokens = null;
			_listOfFunctions = null;
			_listOfFunctionsSorted = null;
		}
	}
}
