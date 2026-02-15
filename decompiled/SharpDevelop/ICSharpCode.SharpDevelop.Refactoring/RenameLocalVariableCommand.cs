using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.Refactoring;

public class RenameLocalVariableCommand : AbstractMenuCommand
{
	public override void Run()
	{
		LocalResolveResult localResolveResult = (LocalResolveResult)Owner;
		string text = MessageService.ShowInputBox("${res:SharpDevelop.Refactoring.Rename}", "${res:SharpDevelop.Refactoring.RenameMemberText}", localResolveResult.Field.Name);
		if (localResolveResult.Field.DeclaringType != null && FindReferencesAndRenameHelper.CheckName(localResolveResult.Field.DeclaringType.ProjectContent.Language, text, localResolveResult.Field.Name))
		{
			List<Reference> list = RefactoringService.FindReferences(localResolveResult, null);
			if (list != null)
			{
				FindReferencesAndRenameHelper.RenameReferences(list, text);
			}
		}
	}
}
