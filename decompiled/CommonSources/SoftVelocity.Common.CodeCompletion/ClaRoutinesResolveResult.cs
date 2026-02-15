using System.Collections;
using ICSharpCode.SharpDevelop.Dom;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaRoutinesResolveResult : ResolveResult
{
	public ClaRoutinesResolveResult(ClaMethod callingMethod)
		: base(callingMethod.DeclaringType, (IMember)(object)callingMethod, (IReturnType)null)
	{
	}

	public override ArrayList GetCompletionData(IProjectContent projectContent)
	{
		ClaMethod claMethod = (ClaMethod)(object)((ResolveResult)this).CallingMember;
		ArrayList arrayList = new ArrayList();
		foreach (ClaRoutine routine in claMethod.Routines)
		{
			arrayList.Add(routine);
		}
		return arrayList;
	}
}
