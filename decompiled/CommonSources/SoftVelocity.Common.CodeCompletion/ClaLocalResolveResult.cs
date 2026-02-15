using System.Collections;
using ICSharpCode.SharpDevelop.Dom;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaLocalResolveResult : LocalResolveResult
{
	public ClaLocalResolveResult(IMember callingMember, IField field)
		: base(callingMember, field)
	{
	}

	public override ArrayList GetCompletionData(IProjectContent projectContent)
	{
		ArrayList completionData = ((ResolveResult)this).GetCompletionData(projectContent);
		ClaResolveResult.AddInterfaces(completionData, ((ResolveResult)this).ResolvedType);
		if (((ResolveResult)this).ResolvedType != null && ((ResolveResult)this).ResolvedType.GetUnderlyingClass() is ClaClass claClass)
		{
			ClaTypeResolveResult.AddMembersToFILE(completionData, claClass, checkTypeOnly: false);
			foreach (IClass innerClass in claClass.InnerClasses)
			{
				if (innerClass is ClaClass && !((ClaClass)(object)innerClass).IsTypeOnly)
				{
					completionData.Add(innerClass);
				}
			}
		}
		return completionData;
	}
}
