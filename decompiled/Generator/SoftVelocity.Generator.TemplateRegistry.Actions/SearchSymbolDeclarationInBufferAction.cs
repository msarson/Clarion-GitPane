using ICSharpCode.TextEditor;
using SearchAndReplace;

namespace SoftVelocity.Generator.TemplateRegistry.Actions;

public class SearchSymbolDeclarationInBufferAction : SearchSymbolDeclarationInFilesAction
{
	protected override void WritebackSearchOptions(string symbolToSearch, SymbolsOnCaretType symbolToSearchType, TextArea textArea)
	{
		base.WritebackSearchOptions(symbolToSearch, symbolToSearchType, textArea);
		SearchOptions.SearchAndReplaceBinding = SearchOptions.CurrentDocumentBinding;
		SearchOptions.MultiLineMatch = false;
	}
}
