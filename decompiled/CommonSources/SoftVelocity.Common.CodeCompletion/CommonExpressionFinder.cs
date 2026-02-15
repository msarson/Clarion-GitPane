using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public abstract class CommonExpressionFinder : IExpressionFinder
{
	private string fileName;

	private int curLine;

	private int curColumn;

	private ClaCompilationUnit cu;

	private object nearestObj;

	private int lastExpressionStartPosition;

	private static Regex newLine = new Regex("(\\n\\r?)|(\\r\\n?)", RegexOptions.Multiline | RegexOptions.Compiled);

	protected int initialOffset;

	protected string text;

	protected int offset;

	protected static int Err = 0;

	protected static int Dot = 1;

	protected static int StrLit = 2;

	protected static int Ident = 3;

	protected static int New = 4;

	protected static int Bracket = 5;

	protected static int Parent = 6;

	protected static int Curly = 7;

	protected static int Using = 8;

	protected static int Digit = 9;

	protected int curTokenType;

	protected static readonly string[] tokenStateName = new string[10] { "Err", "Dot", "StrLit", "Ident", "New", "Bracket", "Paren", "Curly", "Using", "Digit" };

	protected string lastIdentifier;

	protected static readonly int ERROR = 0;

	protected static readonly int START = 1;

	protected static readonly int DOT = 2;

	protected static readonly int MORE = 3;

	protected static readonly int CURLY = 4;

	protected static readonly int CURLY2 = 5;

	protected static readonly int CURLY3 = 6;

	protected static readonly int ACCEPT = 7;

	protected static readonly int ACCEPTNOMORE = 8;

	protected static readonly int ACCEPT2 = 9;

	protected static readonly string[] stateName = new string[10] { "ERROR", "START", "DOT", "MORE", "CURLY", "CURLY2", "CURLY3", "ACCEPT", "ACCEPTNOMORE", "ACCEPT2" };

	protected int state;

	protected int lastAccept;

	protected static int[,] stateTable = new int[10, 10]
	{
		{ ERROR, ERROR, ERROR, ERROR, ERROR, ERROR, ERROR, ERROR, ERROR, ERROR },
		{ ERROR, DOT, ACCEPT, ACCEPT, ERROR, MORE, ACCEPT2, CURLY, ACCEPTNOMORE, ERROR },
		{ ERROR, ERROR, ACCEPT, ACCEPT, ERROR, MORE, ACCEPT, CURLY, ERROR, ACCEPT },
		{ ERROR, ERROR, ACCEPT, ACCEPT, ERROR, MORE, ACCEPT2, CURLY, ERROR, ACCEPT },
		{ ERROR, ERROR, ERROR, ERROR, ERROR, CURLY2, ERROR, ERROR, ERROR, ERROR },
		{ ERROR, ERROR, ERROR, CURLY3, ERROR, ERROR, ERROR, ERROR, ERROR, CURLY3 },
		{ ERROR, ERROR, ERROR, ERROR, ACCEPTNOMORE, ERROR, ERROR, ERROR, ERROR, ERROR },
		{ ERROR, MORE, ERROR, ERROR, ACCEPT, ERROR, ERROR, ERROR, ACCEPTNOMORE, ERROR },
		{ ERROR, ERROR, ERROR, ERROR, ERROR, ERROR, ERROR, ERROR, ERROR, ERROR },
		{ ERROR, MORE, ERROR, ACCEPT, ACCEPT, ERROR, ERROR, ERROR, ERROR, ACCEPT }
	};

	protected virtual bool ProcessParenthesis => true;

	protected virtual bool ProcessSquareBracket => true;

	protected virtual bool ProcessAngleBracket => true;

	internal int LastExpressionStartPosition => lastExpressionStartPosition;

	public int CurLine => curLine;

	public int CurColumn => curColumn;

	public ClaCompilationUnit CU => cu;

	public object NearestObject => nearestObj;

	public string FileName => fileName;

	protected abstract bool IsHardReservedKeyword(string text);

	protected CommonExpressionFinder(string fileName)
	{
		this.fileName = fileName;
		ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(fileName);
		if (parseInformationIfExist != null && parseInformationIfExist.MostRecentCompilationUnit is ClaCompilationUnit)
		{
			cu = (ClaCompilationUnit)(object)parseInformationIfExist.MostRecentCompilationUnit;
		}
	}

	protected virtual ExpressionResult CreateResult(string expression, string inText, int offset)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (expression == null)
		{
			return new ExpressionResult((string)null);
		}
		return new ExpressionResult(expression);
	}

	public string RemoveLastPart(string expression)
	{
		text = expression;
		offset = text.Length - 1;
		ReadNextToken();
		if (curTokenType == Ident && Peek() == '.')
		{
			GetNext();
		}
		return text.Substring(0, offset + 1);
	}

	public ExpressionResult FindExpression(string inText, int offset)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		InitializePosition(inText, offset);
		inText = FilterComments(inText, ref offset);
		return CreateResult(FindExpressionInternal(inText, offset), inText, offset);
	}

	protected virtual string FindExpressionInternal(string inText, int offset)
	{
		text = inText;
		this.offset = (lastAccept = offset);
		state = START;
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		while (state != ERROR)
		{
			ReadNextToken();
			state = stateTable[state, curTokenType];
			if (state == ACCEPT || state == ACCEPT2)
			{
				lastAccept = this.offset;
			}
			if (lastAccept < 0)
			{
				state = ACCEPTNOMORE;
			}
			if (state == ACCEPTNOMORE)
			{
				lastExpressionStartPosition = this.offset + 1;
				return text.Substring(this.offset + 1, offset - this.offset);
			}
		}
		if (lastAccept < 0)
		{
			return null;
		}
		lastExpressionStartPosition = lastAccept + 1;
		return text.Substring(lastAccept + 1, offset - lastAccept);
	}

	public ExpressionResult FindFullExpression(string inText, int offset)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		InitializePosition(inText, offset);
		int num = offset;
		string inText2 = FilterComments(inText, ref num);
		string value = FindExpressionInternal(inText2, num);
		if (string.IsNullOrEmpty(value))
		{
			return CreateResult(null, inText2, num);
		}
		StringBuilder stringBuilder = new StringBuilder(value);
		bool flag = false;
		int i;
		for (i = offset + 1; i < inText.Length; i++)
		{
			char c = inText[i];
			if (IsIdentifierPart(c))
			{
				if (char.IsWhiteSpace(inText, i - 1))
				{
					flag = true;
					break;
				}
				stringBuilder.Append(c);
			}
			else if (c == '#' || c == '$' || c == '"')
			{
				if (!IsIdentifierPart(inText[i - 1]))
				{
					break;
				}
				stringBuilder.Append(c);
			}
			else
			{
				if (char.IsWhiteSpace(c))
				{
					continue;
				}
				if ((c == '(' && ProcessParenthesis) || (c == '[' && ProcessSquareBracket))
				{
					int num2 = SearchBracketForward(inText, i + 1, c, (c == '(') ? ')' : ']');
					if (num2 < 0)
					{
						break;
					}
					if (c == '[')
					{
						bool flag2 = false;
						for (int j = i + 1; j < num2; j++)
						{
							if (inText[j] != ',' && !char.IsWhiteSpace(inText, j))
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							break;
						}
					}
					stringBuilder.Append(inText, i, num2 - i + 1);
					break;
				}
				if (c != '<' || !ProcessAngleBracket)
				{
					break;
				}
				int num3 = FindEndOfTypeParameters(inText, i);
				if (num3 < 0)
				{
					break;
				}
				stringBuilder.Append(inText, i, num3 - i + 1);
				i = num3;
			}
		}
		ExpressionResult result = CreateResult(stringBuilder.ToString(), inText2, num);
		if (result.Context == ExpressionContext.Default && flag)
		{
			stringBuilder = new StringBuilder();
			for (; i < inText.Length; i++)
			{
				char c2 = inText[i];
				if (!char.IsLetterOrDigit(c2) && c2 != '_' && c2 != ':')
				{
					break;
				}
				stringBuilder.Append(c2);
			}
			if (stringBuilder.Length > 0 && !IsHardReservedKeyword(stringBuilder.ToString()))
			{
				result.Context = ExpressionContext.Type;
			}
		}
		return result;
	}

	private void InitializePosition(string text, int offset)
	{
		curLine = 1;
		int num = 0;
		foreach (Match item in newLine.Matches(text))
		{
			if (item.Index <= offset)
			{
				num = item.Index + item.Length;
				curLine++;
				continue;
			}
			break;
		}
		curColumn = offset - num + 1;
		if (cu != null)
		{
			nearestObj = cu.FindNearestObject(curLine, curColumn);
		}
	}

	private static int FindEndOfTypeParameters(string inText, int offset)
	{
		int num = 0;
		bool flag = false;
		for (int i = offset; i < inText.Length; i++)
		{
			char c = inText[i];
			if (!char.IsWhiteSpace(c))
			{
				if (char.IsLetterOrDigit(c))
				{
					flag = true;
				}
				else
				{
					switch (c)
					{
					case ',':
					case '?':
					case '[':
					case ']':
						flag = true;
						break;
					case '<':
						num++;
						break;
					case '>':
						num--;
						break;
					default:
						return -1;
					}
				}
			}
			if (num == 0)
			{
				if (flag)
				{
					return i;
				}
				return -1;
			}
		}
		return -1;
	}

	private static int SearchBracketForward(string text, int offset, char openBracket, char closingBracket)
	{
		int num = 1;
		while (offset < text.Length)
		{
			char c = text[offset];
			int num2 = -1;
			if (c == '!' || c == '\'' || c == '@')
			{
				num2 = GetBlockLenght(text, offset);
				if (num2 != -1)
				{
					offset += num2;
					continue;
				}
			}
			if (c == openBracket)
			{
				num++;
			}
			else if (c == closingBracket)
			{
				num--;
				if (num == 0)
				{
					return offset;
				}
			}
			offset++;
		}
		return -1;
	}

	private static int GetBlockLenght(string text, int offset)
	{
		string input = text.Substring(offset);
		Match match = Regex.Match(input, "^!~[.\n]*~!", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^!.*", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^'[^']*'", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^@N(\\$|(~[^~]*?~))?(-|(?<cazzoPar>\\())?(0|_|\\*)?[0-9]+(-|_|(?<cazzoDot>\\.))?(?(cazzoDot)((v|`|\\.)?[0-9]+)|((v|`|\\.)[0-9]+))?(?(cazzoPar)\\)|-?)($$|~[^~]*?~)?B?", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^@E[0-9]+(\\.|_|`)\\.?[0-9]+B?", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^@S[0-9]+", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^@D[0-9]+(\\.|`|-|_)?((<|>)[0-9]*)?B?", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^@T[0-9]+(\\.|`|-|_)?B?", RegexOptions.IgnoreCase);
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^@P[^P]*P(B|b)?");
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^@p[^p]*p(B|b)?");
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^@K[^K]*K(B|b)?");
		if (match.Success)
		{
			return match.Length;
		}
		match = Regex.Match(input, "^@k[^k]*k(B|b)?");
		if (match.Success)
		{
			return match.Length;
		}
		return -1;
	}

	public virtual string FilterComments(string text, ref int offset)
	{
		if (offset == 0)
		{
			return text;
		}
		if (text.Length <= offset)
		{
			return null;
		}
		initialOffset = offset;
		StringBuilder stringBuilder = new StringBuilder();
		int curOffset = 0;
		while (curOffset <= initialOffset)
		{
			char c = text[curOffset];
			switch (c)
			{
			case '\'':
				stringBuilder.Append(c);
				curOffset++;
				if (!ReadString(stringBuilder, text, ref curOffset))
				{
					return null;
				}
				break;
			case '!':
				offset--;
				curOffset++;
				if (!ReadToEOL(text, ref curOffset, ref offset))
				{
					return null;
				}
				break;
			default:
				stringBuilder.Append(c);
				curOffset++;
				break;
			}
		}
		return stringBuilder.ToString();
	}

	protected bool ReadToEOL(string text, ref int curOffset, ref int offset)
	{
		while (curOffset <= initialOffset)
		{
			char c = text[curOffset++];
			offset--;
			if (c == '\n')
			{
				return true;
			}
		}
		return false;
	}

	protected bool ReadString(StringBuilder outText, string text, ref int curOffset)
	{
		while (curOffset <= initialOffset)
		{
			char c = text[curOffset++];
			outText.Append(c);
			if (c == '\'')
			{
				return true;
			}
		}
		return false;
	}

	protected char GetNext()
	{
		if (offset >= 0)
		{
			return text[offset--];
		}
		return '\0';
	}

	protected char GetNextNonWhiteSpace()
	{
		char next;
		do
		{
			next = GetNext();
		}
		while (char.IsWhiteSpace(next));
		return next;
	}

	protected char Peek(int n)
	{
		if (offset - n >= 0)
		{
			return text[offset - n];
		}
		return '\0';
	}

	protected char Peek()
	{
		if (offset >= 0)
		{
			return text[offset];
		}
		return '\0';
	}

	protected void UnGet()
	{
		offset++;
	}

	protected void UnGetToken()
	{
		do
		{
			UnGet();
		}
		while (char.IsLetterOrDigit(Peek()));
	}

	protected static string GetTokenName(int state)
	{
		return tokenStateName[state];
	}

	protected void ReadNextToken()
	{
		char next = GetNext();
		curTokenType = Err;
		if (next == '\0' || next == '\n' || next == '\r')
		{
			return;
		}
		while (char.IsWhiteSpace(next))
		{
			next = GetNext();
			if (next == '\n' || next == '\r')
			{
				return;
			}
		}
		switch (next)
		{
		case '}':
			if (ReadBracket('{', '}'))
			{
				curTokenType = Curly;
			}
			return;
		case ')':
			if (ReadBracket('(', ')'))
			{
				curTokenType = Parent;
			}
			return;
		case ']':
			if (ReadBracket('[', ']'))
			{
				curTokenType = Bracket;
			}
			return;
		case '.':
			curTokenType = Dot;
			return;
		case '>':
			if (ReadTypeParameters())
			{
				ReadNextToken();
			}
			return;
		case '\'':
			if (ReadStringLiteral(next))
			{
				curTokenType = StrLit;
			}
			return;
		case '"':
		case '#':
		case '$':
		{
			char c = next;
			next = GetNext();
			if (IsIdentifierPart(next))
			{
				string text = ReadIdentifier(next);
				if (text != null)
				{
					curTokenType = Ident;
					lastIdentifier = text + c;
				}
			}
			return;
		}
		}
		if (IsDigit())
		{
			ReadDigit(next);
			curTokenType = Digit;
			return;
		}
		if (!IsIdentifierPart(next))
		{
			return;
		}
		string text2 = ReadIdentifier(next);
		if (text2 != null)
		{
			switch (text2.ToLower())
			{
			case "new":
				curTokenType = New;
				break;
			case "using":
			case "namespace":
				curTokenType = Using;
				break;
			default:
				curTokenType = Ident;
				lastIdentifier = text2;
				break;
			}
		}
	}

	private bool ReadTypeParameters()
	{
		int num = 1;
		while (num > 0)
		{
			char next = GetNext();
			switch (next)
			{
			case '<':
				num--;
				continue;
			case '>':
				num++;
				continue;
			case ',':
			case '?':
			case '[':
			case ']':
				continue;
			}
			if (!char.IsWhiteSpace(next) && !IsIdentifierPart(next))
			{
				return false;
			}
		}
		return true;
	}

	protected bool IsDigit()
	{
		int num = 0;
		char c;
		while (true)
		{
			c = Peek(num);
			if (!char.IsDigit(c))
			{
				break;
			}
			num++;
		}
		if (num > 0)
		{
			return !char.IsLetter(c);
		}
		return false;
	}

	protected bool ReadStringLiteral(char litStart)
	{
		char next;
		do
		{
			next = GetNext();
			if (next == '\0')
			{
				return false;
			}
		}
		while (next != litStart);
		return true;
	}

	protected bool ReadBracket(char openBracket, char closingBracket)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		switch (openBracket)
		{
		case '(':
			num3++;
			break;
		case '[':
			num2++;
			break;
		case '{':
			num++;
			break;
		}
		while (num3 != 0 || num2 != 0 || num != 0)
		{
			char next = GetNext();
			if (next == '\0')
			{
				return false;
			}
			switch (next)
			{
			case '(':
				num3--;
				break;
			case '[':
				num2--;
				break;
			case '{':
				num--;
				break;
			case ')':
				num3++;
				break;
			case ']':
				num2++;
				break;
			case '}':
				num++;
				break;
			}
		}
		return true;
	}

	protected string ReadDigit(char ch)
	{
		string text = ch.ToString();
		while (char.IsDigit(Peek()) || Peek() == '.')
		{
			text = GetNext() + text;
		}
		return text;
	}

	protected string ReadIdentifier(char ch)
	{
		string text = ch.ToString();
		while (IsIdentifierPart(Peek()))
		{
			text = GetNext() + text;
		}
		return text;
	}

	protected static bool IsIdentifierPart(char ch)
	{
		if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != ':' && ch != '?')
		{
			return ch == '@';
		}
		return true;
	}

	protected static string GetStateName(int state)
	{
		return stateName[state];
	}
}
