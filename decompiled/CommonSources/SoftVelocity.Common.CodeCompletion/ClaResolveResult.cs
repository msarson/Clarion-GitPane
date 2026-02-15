using System.Collections;
using ICSharpCode.SharpDevelop.Dom;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaResolveResult : ResolveResult
{
	public ClaResolveResult(IClass callingClass, IMember callingMethod, IReturnType resolvedType)
		: base(callingClass, callingMethod, resolvedType)
	{
	}

	public override ArrayList GetCompletionData(IProjectContent projectContent)
	{
		ArrayList completionData = ((ResolveResult)this).GetCompletionData(projectContent);
		AddInterfaces(completionData, ((ResolveResult)this).ResolvedType);
		if (((ResolveResult)this).ResolvedType != null && ((ResolveResult)this).ResolvedType.GetUnderlyingClass() is ClaClass claClass)
		{
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

	public static void AddInterfaces(ArrayList res, IReturnType resolvedType)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		if (res == null || resolvedType == null)
		{
			return;
		}
		IClass underlyingClass = resolvedType.GetUnderlyingClass();
		if (underlyingClass == null)
		{
			return;
		}
		foreach (IClass item in underlyingClass.ClassInheritanceTree)
		{
			if (underlyingClass != item && (int)item.ClassType == 2)
			{
				res.Add(item);
			}
		}
	}
}
