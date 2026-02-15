using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Clarion.Core.Redirection;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using SearchAndReplace;

namespace SoftVelocity.Generator.TemplateRegistry.Actions;

public abstract class BaseSymbolSearchRedFileAction : AbstractEditAction
{
	protected enum SymbolsOnCaretType
	{
		Symbol,
		At,
		Embed
	}

	private string _OldLookIn;

	private string _OldLookInFiletypes;

	private string _OldReplacePattern;

	private string _OldFindPattern;

	private bool _OldMatchCase;

	private bool _OldMatchWholeWord;

	private bool _OldIncludeSubdirectories;

	private bool _OldMultiLineMatch;

	private bool _OldShowResults;

	private SearchStrategyType _OldSearchStrategyType;

	private AbstractSearchAndReplaceBinding _OldSearchAndReplaceBinding;

	private void StoreSearchOptions()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_OldShowResults = SearchOptions.ShowResults;
		_OldFindPattern = SearchOptions.FindPattern;
		_OldSearchStrategyType = SearchOptions.SearchStrategyType;
		_OldLookIn = SearchOptions.LookIn;
		_OldLookInFiletypes = SearchOptions.LookInFiletypes;
		_OldReplacePattern = SearchOptions.ReplacePattern;
		_OldMatchCase = SearchOptions.MatchCase;
		_OldMatchWholeWord = SearchOptions.MatchWholeWord;
		_OldIncludeSubdirectories = SearchOptions.IncludeSubdirectories;
		_OldMultiLineMatch = SearchOptions.MultiLineMatch;
		_OldSearchAndReplaceBinding = SearchOptions.SearchAndReplaceBinding;
	}

	private void RestoreSearchOptions()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		SearchOptions.FindPattern = _OldFindPattern;
		SearchOptions.SearchStrategyType = _OldSearchStrategyType;
		SearchOptions.LookIn = _OldLookIn;
		SearchOptions.LookInFiletypes = _OldLookInFiletypes;
		SearchOptions.ReplacePattern = _OldReplacePattern;
		SearchOptions.MatchCase = _OldMatchCase;
		SearchOptions.MatchWholeWord = _OldMatchWholeWord;
		SearchOptions.IncludeSubdirectories = _OldIncludeSubdirectories;
		SearchOptions.MultiLineMatch = _OldMultiLineMatch;
		SearchOptions.SearchAndReplaceBinding = _OldSearchAndReplaceBinding;
		SearchOptions.ShowResults = _OldShowResults;
	}

	public BaseSymbolSearchRedFileAction()
	{
	}

	public override void Execute(TextArea textArea)
	{
		if (GetSymbolsOnCaret(textArea, out var symbol, out var symboltype) && !string.IsNullOrEmpty(symbol) && PreProcessSymbolToSearch(symbol, textArea))
		{
			StoreSearchOptions();
			if (WritebackBaseSearchOptions(symbol, textArea))
			{
				WritebackSearchOptions(symbol, symboltype, textArea);
				ExecuteSearch(symbol, textArea);
				SearchInFilesManager.GoToFirstResultIfUnique();
			}
			RestoreSearchOptions();
		}
	}

	private void FillFiles(StringBuilder sb, bool forWin)
	{
		RedirectionFile activeRedirectionFile = RedirectionFile.GetActiveRedirectionFile(forWin);
		Dictionary<string, List<string>> dictionary = activeRedirectionFile.EvaluatedPaths("*.tpl;*.tpw", RedirectionFile.CurrentDirectory);
		foreach (string key in dictionary.Keys)
		{
			foreach (string item in dictionary[key])
			{
				if (!Directory.Exists(item))
				{
					continue;
				}
				string[] array = key.Split(';');
				string[] array2 = array;
				foreach (string searchPattern in array2)
				{
					string[] files = Directory.GetFiles(item, searchPattern);
					string[] array3 = files;
					foreach (string value in array3)
					{
						sb.Append(value);
						sb.Append(";");
					}
				}
			}
		}
	}

	protected virtual bool WritebackBaseSearchOptions(string symbolToSearch, TextArea textArea)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FillFiles(stringBuilder, forWin: false);
		FillFiles(stringBuilder, forWin: true);
		if (stringBuilder.Length == 0)
		{
			return false;
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		SearchOptions.LookIn = stringBuilder.ToString();
		SearchOptions.ReplacePattern = "";
		SearchOptions.MatchCase = false;
		SearchOptions.MatchWholeWord = false;
		SearchOptions.IncludeSubdirectories = true;
		SearchOptions.SearchAndReplaceBinding = (AbstractSearchAndReplaceBinding)(object)TemplateRegistrySearchBinding.Instance;
		SearchOptions.ShowResults = false;
		return true;
	}

	protected abstract void WritebackSearchOptions(string symbolToSearch, SymbolsOnCaretType symbolToSearchType, TextArea textArea);

	protected abstract void ExecuteSearch(string symbolToSearch, TextArea textArea);

	protected virtual bool PreProcessSymbolToSearch(string symbolToSearch, TextArea textArea)
	{
		return true;
	}

	protected bool GetSymbolsOnCaret(TextArea textArea, out string symbol, out SymbolsOnCaretType symboltype)
	{
		symboltype = SymbolsOnCaretType.Symbol;
		TextEditorControl motherTextEditorControl = textArea.MotherTextEditorControl;
		IDocument document = ((TextEditorControlBase)motherTextEditorControl).Document;
		int num = document.GetLineNumberForOffset(((TextEditorControlBase)motherTextEditorControl).ActiveTextAreaControl.Caret.Offset) + 1;
		int num2 = ((TextEditorControlBase)motherTextEditorControl).ActiveTextAreaControl.Caret.Offset - document.GetLineSegment(num - 1).Offset + 1;
		if (((TextEditorControlBase)motherTextEditorControl).ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
		{
			num2 -= ((TextEditorControlBase)motherTextEditorControl).ActiveTextAreaControl.SelectionManager.SelectedText.Length;
		}
		symbol = string.Empty;
		bool flag = false;
		LineSegment lineSegment = document.GetLineSegment(num - 1);
		TextWord word = lineSegment.GetWord(num2);
		if (word != null)
		{
			string word2 = word.Word;
			int offset = word.Offset;
			if (offset > 0)
			{
				TextWord word3 = lineSegment.GetWord(offset - 1);
				if (word3 != null && word3.Word == "%")
				{
					symbol = word2;
					flag = true;
				}
			}
			if (flag && offset > 3)
			{
				offset -= 2;
				TextWord val = null;
				while (offset > -1)
				{
					val = lineSegment.GetWord(offset);
					if (!val.IsWhiteSpace && val.Word != "(")
					{
						break;
					}
					offset--;
				}
				if (offset > 0 && val != null)
				{
					TextWord word4 = lineSegment.GetWord(offset - 1);
					if (word4 != null && word4.Word == "#")
					{
						if (val.Word.Equals("AT", StringComparison.OrdinalIgnoreCase))
						{
							symboltype = SymbolsOnCaretType.At;
						}
						else if (val.Word.Equals("EMBED", StringComparison.OrdinalIgnoreCase))
						{
							symboltype = SymbolsOnCaretType.Embed;
						}
					}
				}
			}
		}
		return flag;
	}
}
