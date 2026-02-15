using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.Refactoring;

public class RemoveUnusedUsingsCommand : AbstractRefactoringCommand
{
	protected override void Run(TextEditorControl textEditor, RefactoringProvider provider)
	{
		NamespaceRefactoringService.ManageUsings(textEditor.FileName, textEditor.Document, sort: true, removedUnused: true);
	}
}
