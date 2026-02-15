using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common;

public abstract class CommonRefactoringProvider : RefactoringProvider
{
	public virtual bool SupportAddUsingDeclaration => false;

	public virtual void AddUsingDeclaration(ClaCompilationUnit cu, IDocument doc, string newNamespace, bool sortExistingUsings)
	{
	}
}
