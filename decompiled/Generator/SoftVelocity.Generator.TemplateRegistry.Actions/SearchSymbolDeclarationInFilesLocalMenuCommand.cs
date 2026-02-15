using ICSharpCode.SharpDevelop.DefaultEditor.Commands;
using ICSharpCode.TextEditor.Actions;

namespace SoftVelocity.Generator.TemplateRegistry.Actions;

public class SearchSymbolDeclarationInFilesLocalMenuCommand : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => (IEditAction)(object)new SearchSymbolDeclarationInFilesLocalAction();
}
