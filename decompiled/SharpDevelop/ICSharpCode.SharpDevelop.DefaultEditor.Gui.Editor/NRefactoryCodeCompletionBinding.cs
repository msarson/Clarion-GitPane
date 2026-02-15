using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public abstract class NRefactoryCodeCompletionBinding : DefaultCodeCompletionBinding
{
	protected class InspectedCall
	{
		internal Location start;

		internal List<Location> commas = new List<Location>();

		internal InspectedCall parent;

		public InspectedCall(Location start, InspectedCall parent)
		{
			this.start = start;
			this.parent = parent;
		}
	}

	private class ContextCompletionDataProvider : CachedCompletionDataProvider
	{
		internal char activationKey;

		internal ContextCompletionDataProvider(ICompletionDataProvider baseProvider)
			: base(baseProvider)
		{
		}

		public override CompletionDataProviderKeyResult ProcessKey(char key)
		{
			if (key == '=' && activationKey == '=')
			{
				return CompletionDataProviderKeyResult.BeforeStartKey;
			}
			activationKey = '\0';
			return base.ProcessKey(key);
		}
	}

	private readonly SupportedLanguage language;

	private readonly int eofToken;

	private readonly int commaToken;

	private readonly int openParensToken;

	private readonly int closeParensToken;

	private readonly int openBracketToken;

	private readonly int closeBracketToken;

	private readonly int openBracesToken;

	private readonly int closeBracesToken;

	private readonly LanguageProperties languageProperties;

	protected NRefactoryCodeCompletionBinding(SupportedLanguage language)
	{
		this.language = language;
		if (language == SupportedLanguage.CSharp)
		{
			eofToken = 0;
			commaToken = 14;
			openParensToken = 20;
			closeParensToken = 21;
			openBracketToken = 18;
			closeBracketToken = 19;
			openBracesToken = 16;
			closeBracesToken = 17;
			languageProperties = LanguageProperties.CSharp;
		}
		else
		{
			eofToken = 0;
			commaToken = 12;
			openParensToken = 24;
			closeParensToken = 25;
			openBracketToken = -1;
			closeBracketToken = -1;
			openBracesToken = 22;
			closeBracesToken = 23;
			languageProperties = LanguageProperties.VBNet;
		}
	}

	protected IList<ResolveResult> ResolveCallParameters(SharpDevelopTextAreaControl editor, InspectedCall call)
	{
		List<ResolveResult> list = new List<ResolveResult>();
		int num = LocationToOffset(editor, call.start);
		string text = editor.Text;
		int num2;
		foreach (Location comma in call.commas)
		{
			num2 = LocationToOffset(editor, comma);
			if (num2 >= 0)
			{
				string text2 = editor.Document.GetText(num + 1, num2 - (num + 1));
				list.Add(ParserService.Resolve(new ExpressionResult(text2), comma.Line, comma.Column, editor.FileName, text));
				continue;
			}
			break;
		}
		num2 = editor.ActiveTextAreaControl.Caret.Offset;
		if (num < num2)
		{
			string text3 = editor.Document.GetText(num + 1, num2 - (num + 1));
			list.Add(ParserService.Resolve(new ExpressionResult(text3), editor.ActiveTextAreaControl.Caret.Line + 1, editor.ActiveTextAreaControl.Caret.Column + 1, editor.FileName, text));
		}
		return list;
	}

	protected bool InsightRefreshOnComma(SharpDevelopTextAreaControl editor, char ch)
	{
		NRefactoryResolver nRefactoryResolver = new NRefactoryResolver(ParserService.CurrentProjectContent, languageProperties);
		Location location = new Location(editor.ActiveTextAreaControl.Caret.Column + 1, editor.ActiveTextAreaControl.Caret.Line + 1);
		if (nRefactoryResolver.Initialize(editor.FileName, location.Y, location.X))
		{
			TextReader textReader = nRefactoryResolver.ExtractCurrentMethod(editor.Text);
			if (textReader != null)
			{
				ILexer lexer = ParserFactory.CreateLexer(language, textReader);
				InspectedCall inspectedCall = new InspectedCall(Location.Empty, null);
				inspectedCall.parent = inspectedCall;
				Token token;
				while ((token = lexer.NextToken()) != null && token.kind != eofToken && token.Location < location)
				{
					if (token.kind == commaToken)
					{
						inspectedCall.commas.Add(token.Location);
					}
					else if (token.kind == openParensToken || token.kind == openBracketToken || token.kind == openBracesToken)
					{
						inspectedCall = new InspectedCall(token.Location, inspectedCall);
					}
					else if (token.kind == closeParensToken || token.kind == closeBracketToken || token.kind == closeBracesToken)
					{
						inspectedCall = inspectedCall.parent;
					}
				}
				int num = LocationToOffset(editor, inspectedCall.start);
				if (num >= 0 && num < editor.Document.TextLength)
				{
					switch (editor.Document.GetCharAt(num))
					{
					case '(':
						ShowInsight(editor, new MethodInsightDataProvider(num, setupOnlyOnce: true), ResolveCallParameters(editor, inspectedCall), ch);
						return true;
					case '[':
						ShowInsight(editor, new IndexerInsightDataProvider(num, setupOnlyOnce: true), ResolveCallParameters(editor, inspectedCall), ch);
						return true;
					}
					LoggingService.Warn("Expected '(' or '[' at start position");
				}
			}
		}
		return false;
	}

	protected bool ProvideContextCompletion(SharpDevelopTextAreaControl editor, IReturnType expected, char charTyped)
	{
		if (expected == null)
		{
			return false;
		}
		IClass underlyingClass = expected.GetUnderlyingClass();
		if (underlyingClass == null)
		{
			return false;
		}
		if (underlyingClass.ClassType == ClassType.Enum)
		{
			CtrlSpaceCompletionDataProvider ctrlSpaceCompletionDataProvider = new CtrlSpaceCompletionDataProvider();
			ctrlSpaceCompletionDataProvider.ForceNewExpression = true;
			ContextCompletionDataProvider contextCompletionDataProvider = new ContextCompletionDataProvider(ctrlSpaceCompletionDataProvider);
			contextCompletionDataProvider.activationKey = charTyped;
			contextCompletionDataProvider.GenerateCompletionData(editor.FileName, editor.ActiveTextAreaControl.TextArea, charTyped);
			ICompletionData[] completionData = contextCompletionDataProvider.CompletionData;
			Array.Sort(completionData);
			for (int i = 0; i < completionData.Length; i++)
			{
				if (completionData[i] is CodeCompletionData { Class: not null } codeCompletionData && codeCompletionData.Class.FullyQualifiedName == expected.FullyQualifiedName)
				{
					contextCompletionDataProvider.DefaultIndex = i;
					break;
				}
			}
			if (contextCompletionDataProvider.DefaultIndex >= 0)
			{
				if (charTyped != ' ')
				{
					ctrlSpaceCompletionDataProvider.InsertSpace = true;
				}
				editor.ShowCompletionWindow(contextCompletionDataProvider, charTyped);
				return true;
			}
		}
		return false;
	}

	protected void ShowInsight(SharpDevelopTextAreaControl editor, MethodInsightDataProvider dp, ICollection<ResolveResult> parameters, char charTyped)
	{
		int count = parameters.Count;
		dp.SetupDataProvider(editor.FileName, editor.ActiveTextAreaControl.TextArea);
		List<IMethodOrProperty> methods = dp.Methods;
		if (methods.Count == 0)
		{
			return;
		}
		bool acceptableMatch;
		if (methods.Count == 1)
		{
			acceptableMatch = true;
			dp.DefaultIndex = 0;
		}
		else
		{
			IReturnType[] array = new IReturnType[count + 1];
			int num = 0;
			foreach (ResolveResult parameter2 in parameters)
			{
				if (parameter2 != null)
				{
					array[num] = parameter2.ResolvedType;
				}
				num++;
			}
			IReturnType[][] inferredTypeParameters;
			int[] array2 = MemberLookupHelper.RankOverloads(methods, array, allowAdditionalArguments: true, out acceptableMatch, out inferredTypeParameters);
			bool flag = false;
			int num2 = -1;
			int defaultIndex = 0;
			for (num = 0; num < array2.Length; num++)
			{
				if (array2[num] > num2)
				{
					num2 = array2[num];
					defaultIndex = num;
					flag = false;
				}
				else if (array2[num] == num2)
				{
					flag = true;
				}
			}
			if (flag)
			{
				acceptableMatch = false;
			}
			dp.DefaultIndex = defaultIndex;
		}
		editor.ShowInsightWindow(dp);
		if (acceptableMatch)
		{
			IMethodOrProperty methodOrProperty = methods[dp.DefaultIndex];
			if (count < methodOrProperty.Parameters.Count)
			{
				IParameter parameter = methodOrProperty.Parameters[count];
				ProvideContextCompletion(editor, parameter.ReturnType, charTyped);
			}
		}
	}

	protected int LocationToOffset(SharpDevelopTextAreaControl editor, Location loc)
	{
		if (loc.IsEmpty || loc.Line - 1 >= editor.Document.TotalNumberOfLines)
		{
			return -1;
		}
		LineSegment lineSegment = editor.Document.GetLineSegment(loc.Line - 1);
		return lineSegment.Offset + Math.Min(loc.Column, lineSegment.Length) - 1;
	}

	protected IMember GetCurrentMember(SharpDevelopTextAreaControl editor)
	{
		Caret caret = editor.ActiveTextAreaControl.Caret;
		NRefactoryResolver nRefactoryResolver = new NRefactoryResolver(ParserService.CurrentProjectContent, languageProperties);
		if (nRefactoryResolver.Initialize(editor.FileName, caret.Line + 1, caret.Column + 1))
		{
			return nRefactoryResolver.CallingMember;
		}
		return null;
	}
}
