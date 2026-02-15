using ICSharpCode.TextEditor;
using SearchAndReplace;

namespace SoftVelocity.Generator.TemplateRegistry.Actions;

public class SearchSymbolDeclarationInFilesAction : BaseSearchSymbolAction
{
	protected override void WritebackSearchOptions(string symbolToSearch, SymbolsOnCaretType symbolToSearchType, TextArea textArea)
	{
		SearchOptions.FindPattern = "(^\\s*\\#(GROUP|EQUATE|DECLARE|EMBED)\\s*\\(\\s*%" + symbolToSearch + "\\W.*|^\\s*\\#PROMPT\\s*\\(\\s*'.*',.+\\)\\s*,\\s*%" + symbolToSearch + "\\W*.*|^\\s*\\#BUTTON\\s*\\(\\s*'.*'(,.+)?\\).*,\\s*MULTI\\s*\\(\\s*%" + symbolToSearch + "\\W*.*)(?i:)";
		SearchOptions.SearchStrategyType = (SearchStrategyType)1;
		SearchOptions.MultiLineMatch = false;
	}
}
