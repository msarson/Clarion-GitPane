using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.Refactoring;

public class FindLocalVariableReferencesCommand : AbstractMenuCommand
{
	public override void Run()
	{
		LocalResolveResult localResolveResult = (LocalResolveResult)Owner;
		FindReferencesAndRenameHelper.ShowAsSearchResults(StringParser.Parse("${res:SharpDevelop.Refactoring.ReferencesTo}", new string[1, 2] { 
		{
			"Name",
			localResolveResult.Field.Name
		} }), RefactoringService.FindReferences(localResolveResult, null));
	}
}
