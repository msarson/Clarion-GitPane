using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.Refactoring;

public class RenameCommand : AbstractRefactoringCommand
{
	protected override void Run(TextEditorControl textEditor, RefactoringProvider provider)
	{
		ResolveResult resolveResult = ResolveAtCaret(textEditor);
		if (resolveResult is MixedResolveResult)
		{
			resolveResult = (resolveResult as MixedResolveResult).PrimaryResult;
		}
		if (resolveResult is TypeResolveResult)
		{
			IClass resolvedClass = (resolveResult as TypeResolveResult).ResolvedClass;
			if (resolvedClass == null)
			{
				ShowUnknownSymbolError();
			}
			else if (resolvedClass.CompilationUnit.FileName == null)
			{
				ShowNoUserCodeError();
			}
			else
			{
				FindReferencesAndRenameHelper.RenameClass(resolvedClass);
			}
		}
		else if (resolveResult is MemberResolveResult)
		{
			Rename((resolveResult as MemberResolveResult).ResolvedMember);
		}
		else if (resolveResult is MethodResolveResult)
		{
			Rename((resolveResult as MethodResolveResult).GetMethodIfSingleOverload());
		}
		else
		{
			ShowUnknownSymbolError();
		}
	}

	private static void ShowUnknownSymbolError()
	{
		MessageService.ShowMessage("${res:SharpDevelop.Refactoring.CannotRenameElement}");
	}

	private static void ShowNoUserCodeError()
	{
		MessageService.ShowMessage("${res:SharpDevelop.Refactoring.CannotRenameBecauseNotUserCode}");
	}

	private static void Rename(IMember member)
	{
		if (member == null)
		{
			ShowUnknownSymbolError();
		}
		else if (member.DeclaringType.CompilationUnit.FileName == null)
		{
			ShowNoUserCodeError();
		}
		else
		{
			FindReferencesAndRenameHelper.RenameMember(member);
		}
	}
}
