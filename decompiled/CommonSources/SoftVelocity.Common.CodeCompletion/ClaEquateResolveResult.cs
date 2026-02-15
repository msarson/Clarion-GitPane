using System.Collections;
using ICSharpCode.SharpDevelop.Dom;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaEquateResolveResult : MemberResolveResult
{
	public ClaEquateResolveResult(ClaCompilationUnit cu, IClass callingClass, IMember callingMember, string equateName, string declarationText, ClaDomRegion pos)
		: base(callingClass, callingMember, CreateEquateField(cu, equateName, declarationText, pos))
	{
	}

	private static IMember CreateEquateField(ClaCompilationUnit cu, string equateName, string declarationText, ClaDomRegion region)
	{
		ClaEquate claEquate = new ClaEquate(equateName, region, (IClass)(object)cu.GlobalClass);
		claEquate.SetDeclarationText(declarationText, cutLabel: true);
		return (IMember)(object)claEquate;
	}

	public override ArrayList GetCompletionData(IProjectContent projectContent)
	{
		return new ArrayList();
	}
}
