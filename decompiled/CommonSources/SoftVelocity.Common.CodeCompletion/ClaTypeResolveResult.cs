using System.Collections;
using ICSharpCode.SharpDevelop.Dom;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaTypeResolveResult : TypeResolveResult
{
	public ClaTypeResolveResult(IClass callingClass, IMember callingMember, IClass resolvedClass)
		: base(callingClass, callingMember, resolvedClass)
	{
	}

	public ClaTypeResolveResult(IClass callingClass, IMember callingMember, IReturnType resolvedType, IClass resolvedClass)
		: base(callingClass, callingMember, resolvedType, resolvedClass)
	{
	}

	public ClaTypeResolveResult(IClass callingClass, IMember callingMember, IReturnType resolvedType)
		: base(callingClass, callingMember, resolvedType)
	{
	}

	public override ArrayList GetCompletionData(IProjectContent projectContent)
	{
		ArrayList completionData = ((ResolveResult)this).GetCompletionData(projectContent.Language, true);
		if (((TypeResolveResult)this).ResolvedClass != null)
		{
			bool flag = true;
			if (((TypeResolveResult)this).ResolvedClass is ClaClass)
			{
				ClaClass claClass = (ClaClass)(object)((TypeResolveResult)this).ResolvedClass;
				if (claClass.IsTypeOnly && claClass.ClarionType != ClarionType.CLASS && claClass.ClarionType != ClarionType.STRUCT)
				{
					flag = false;
				}
			}
			if (flag)
			{
				foreach (IClass item in ((TypeResolveResult)this).ResolvedClass.ClassInheritanceTree)
				{
					completionData.AddRange(item.InnerClasses);
				}
			}
		}
		ClaClass claClass2 = ((TypeResolveResult)this).ResolvedClass as ClaClass;
		AddMembersToFILE(completionData, claClass2, checkTypeOnly: true);
		if (claClass2 != null && claClass2.Dimensions > 0 && !claClass2.IsTypeOnly && !((ClaCompilationUnit)(object)claClass2.CompilationUnit).IsWin)
		{
			IReturnType array = projectContent.SystemTypes.Array;
			LanguageProperties language = projectContent.Language;
			foreach (IMethod method in array.GetMethods())
			{
				if (language.ShowMember((IMember)(object)method, false) && ((IDecoration)method).IsAccessible(((ResolveResult)this).CallingClass, false))
				{
					completionData.Add(method);
				}
			}
			foreach (IEvent @event in array.GetEvents())
			{
				if (language.ShowMember((IMember)(object)@event, false) && ((IDecoration)@event).IsAccessible(((ResolveResult)this).CallingClass, false))
				{
					completionData.Add(@event);
				}
			}
			foreach (IField field in array.GetFields())
			{
				if (language.ShowMember((IMember)(object)field, false) && ((IDecoration)field).IsAccessible(((ResolveResult)this).CallingClass, false))
				{
					completionData.Add(field);
				}
			}
			foreach (IProperty property in array.GetProperties())
			{
				if (language.ShowMember((IMember)(object)property, false) && ((IDecoration)property).IsAccessible(((ResolveResult)this).CallingClass, false))
				{
					completionData.Add(property);
				}
			}
			if (((ResolveResult)this).CallingClass != null)
			{
				ResolveResult.AddExtensions(language, completionData, ((ResolveResult)this).CallingClass, ((ResolveResult)this).ResolvedType);
			}
		}
		return completionData;
	}

	public static void AddMembersToFILE(ArrayList res, ClaClass c, bool checkTypeOnly)
	{
		if (c == null || (checkTypeOnly && c.IsTypeOnly) || c.ClarionType != ClarionType.FILE)
		{
			return;
		}
		foreach (IClass innerClass in c.InnerClasses)
		{
			if (!(innerClass is ClaClass) || ((ClaClass)(object)innerClass).ClarionType != ClarionType.RECORD)
			{
				continue;
			}
			foreach (IField field in innerClass.Fields)
			{
				res.Add(field);
			}
			{
				foreach (IClass innerClass2 in innerClass.InnerClasses)
				{
					res.Add(innerClass2);
				}
				break;
			}
		}
	}
}
