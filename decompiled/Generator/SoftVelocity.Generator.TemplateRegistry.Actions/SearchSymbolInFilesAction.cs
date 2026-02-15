using ICSharpCode.TextEditor;
using SearchAndReplace;

namespace SoftVelocity.Generator.TemplateRegistry.Actions;

public class SearchSymbolInFilesAction : BaseSearchSymbolAction
{
	protected override void WritebackSearchOptions(string symbolToSearch, SymbolsOnCaretType symbolToSearchType, TextArea textArea)
	{
		SearchOptions.FindPattern = "%" + symbolToSearch;
		SearchOptions.MatchWholeWord = true;
		SearchOptions.MatchCase = false;
		SearchOptions.SearchStrategyType = (SearchStrategyType)0;
	}
}
