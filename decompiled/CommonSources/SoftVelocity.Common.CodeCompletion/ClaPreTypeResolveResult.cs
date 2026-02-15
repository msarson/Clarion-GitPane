using System.Collections;
using ICSharpCode.SharpDevelop.Dom;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaPreTypeResolveResult : TypeResolveResult
{
	public ClaPreTypeResolveResult(IClass callingClass, IMember callingMember, IReturnType resolvedType)
		: base(callingClass, callingMember, resolvedType)
	{
	}

	public override ArrayList GetCompletionData(IProjectContent projectContent)
	{
		ArrayList arrayList = new ArrayList();
		if (((ResolveResult)this).ResolvedType is ClaPreCollectionReturnType)
		{
			ClaPreCollectionReturnType claPreCollectionReturnType = (ClaPreCollectionReturnType)(object)((ResolveResult)this).ResolvedType;
			foreach (IField field in claPreCollectionReturnType.GetFields())
			{
				arrayList.Add(field);
			}
			foreach (IClass innerClass in claPreCollectionReturnType.GetInnerClasses())
			{
				arrayList.Add(innerClass);
			}
		}
		else if (((TypeResolveResult)this).ResolvedClass != null)
		{
			foreach (IField field2 in ((TypeResolveResult)this).ResolvedClass.Fields)
			{
				arrayList.Add(field2);
			}
			foreach (IClass innerClass2 in ((TypeResolveResult)this).ResolvedClass.InnerClasses)
			{
				AddInnerClass(arrayList, innerClass2);
			}
		}
		return arrayList;
	}

	private static void AddInnerClass(IList res, IClass @class)
	{
		res.Add(@class);
		foreach (IField field in @class.Fields)
		{
			res.Add(field);
		}
		foreach (IClass innerClass in @class.InnerClasses)
		{
			AddInnerClass(res, innerClass);
		}
	}
}
