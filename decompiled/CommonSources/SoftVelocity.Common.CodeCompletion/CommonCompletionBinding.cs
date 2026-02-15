using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using ICSharpCode.TextEditor.Gui.InsightWindow;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public abstract class CommonCompletionBinding : DefaultCodeCompletionBinding
{
	protected class BracketInfo
	{
		public static BracketInfo Empty = new BracketInfo('\0', -1, 0);

		public char Bracket;

		public int Pos;

		public int CommasCount;

		public BracketInfo(char bracket, int pos, int commas)
		{
			Bracket = bracket;
			Pos = pos;
			CommasCount = commas;
		}
	}

	public enum CompletionRule
	{
		Upper,
		Lower,
		AsDeclared
	}

	public enum UsingGenerationRule
	{
		Simple,
		InParenthesis,
		InString
	}

	public static readonly string CodeCompletionProperty = "ClarionEditor.CodeCompletion";

	protected abstract bool MergeOverrides { get; }

	protected CommonCompletionBinding()
	{
		((DefaultCodeCompletionBinding)this).EnableXmlCommentCompletion = false;
	}

	public override bool HandleKeyPress(SharpDevelopTextAreaControl editor, char ch)
	{
		switch (ch)
		{
		case '(':
			if (IsInComment(editor))
			{
				return false;
			}
			if (((DefaultCodeCompletionBinding)this).EnableMethodInsight && CodeCompletionOptions.InsightEnabled)
			{
				editor.ShowInsightWindow((IInsightDataProvider)(object)new ClaMethodInsightDataProvider());
				return true;
			}
			break;
		case '.':
			if (((DefaultCodeCompletionBinding)this).EnableDotCompletion)
			{
				editor.ShowCompletionWindow((ICompletionDataProvider)(object)new ClaDotCodeCompletionDataProvider(MergeOverrides), ch);
				return true;
			}
			break;
		case ':':
			if (((DefaultCodeCompletionBinding)this).EnableDotCompletion)
			{
				editor.ShowCompletionWindow((ICompletionDataProvider)(object)new ClaPreCodeCompletionDataProvider(MergeOverrides), ch);
				return true;
			}
			break;
		case ',':
			if (CodeCompletionOptions.InsightRefreshOnComma && CodeCompletionOptions.InsightEnabled && InsightRefreshOnComma(editor, ch))
			{
				return true;
			}
			break;
		case ' ':
		{
			if (!CodeCompletionOptions.KeywordCompletionEnabled)
			{
				return false;
			}
			string wordBeforeCaret = editor.GetWordBeforeCaret();
			if (wordBeforeCaret != null)
			{
				return ((DefaultCodeCompletionBinding)this).HandleKeyword(editor, wordBeforeCaret);
			}
			break;
		}
		}
		return false;
	}

	public override bool HandleKeyword(SharpDevelopTextAreaControl editor, string word)
	{
		if (word.Equals("do", StringComparison.InvariantCultureIgnoreCase))
		{
			if (IsInComment(editor))
			{
				return false;
			}
			if (!IsInMethodCode(editor))
			{
				return false;
			}
			editor.ShowCompletionWindow((ICompletionDataProvider)(object)new ClaRoutinesCompletionDataProvider(MergeOverrides), ' ');
			return true;
		}
		return false;
	}

	protected virtual ClaCompilationUnit FindCompilationUnit(SharpDevelopTextAreaControl editor)
	{
		ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(((TextEditorControlBase)editor).FileName);
		if (parseInformationIfExist != null && parseInformationIfExist.MostRecentCompilationUnit is ClaCompilationUnit)
		{
			return (ClaCompilationUnit)(object)parseInformationIfExist.MostRecentCompilationUnit;
		}
		return null;
	}

	protected bool IsInMethodCode(SharpDevelopTextAreaControl editor)
	{
		ClaCompilationUnit claCompilationUnit = FindCompilationUnit(editor);
		if (claCompilationUnit != null)
		{
			int num = ((TextEditorControlBase)editor).ActiveTextAreaControl.Caret.Line + 1;
			int num2 = ((TextEditorControlBase)editor).ActiveTextAreaControl.Caret.Column + 1;
			object obj = claCompilationUnit.FindNearestObject(num, num2);
			if (obj is ClaMethod && ((ClaMethod)obj).IsInsideCode(num, num2))
			{
				return true;
			}
		}
		return false;
	}

	protected static int FindBestOverloadByParametersCount(int commasCount, List<IMethodOrProperty> methods)
	{
		if (methods.Count == 1)
		{
			return 0;
		}
		int num = commasCount + 1;
		int[] array = new int[methods.Count];
		int num2 = -1;
		for (int i = 0; i < methods.Count; i++)
		{
			int count = methods[i].Parameters.Count;
			if (count == num)
			{
				return i;
			}
			if (count < num)
			{
				array[i] = count - num;
				if (count > 0 && methods[i].Parameters[count - 1].IsParams && num2 == -1)
				{
					num2 = i;
				}
			}
			else
			{
				array[i] = count - num;
			}
		}
		int num3 = int.MaxValue;
		int num4 = int.MinValue;
		int num5 = -1;
		int num6 = -1;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] > 0)
			{
				if (num3 > array[j])
				{
					num3 = array[j];
					num5 = j;
				}
			}
			else if (num4 < array[j])
			{
				num4 = array[j];
				num6 = j;
			}
		}
		if (num5 != -1)
		{
			return num5;
		}
		if (num2 != -1)
		{
			return num2;
		}
		if (num6 != -1)
		{
			return num6;
		}
		return 0;
	}

	protected abstract bool IsInComment(SharpDevelopTextAreaControl editor);

	protected abstract bool InsightRefreshOnComma(SharpDevelopTextAreaControl editor, char ch);
}
