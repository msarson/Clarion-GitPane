using System;
using System.Collections;
using ICSharpCode.SharpDevelop.Dom;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaProcedureDefResolveResult : ResolveResult
{
	private CompoundClass cc;

	private IClass @interface;

	public ClaProcedureDefResolveResult(ClaClass c, CompoundClass cc, IClass @interface)
		: base((IClass)(object)c, (IMember)null, (IReturnType)null)
	{
		this.cc = cc;
		this.@interface = @interface;
	}

	public override ArrayList GetCompletionData(IProjectContent projectContent)
	{
		if (@interface != null)
		{
			return GetFromInterface();
		}
		return GetFromClass();
	}

	private ArrayList GetFromClass()
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		ClaClass claClass = (ClaClass)(object)((ResolveResult)this).CallingClass;
		IClass val = (IClass)(((object)cc) ?? ((object)claClass));
		ArrayList arrayList = new ArrayList();
		foreach (IMethod method in val.Methods)
		{
			if (method is ClaMethod && ((ClaMethod)(object)method).IsUnresolvedDecl)
			{
				arrayList.Add(method);
			}
		}
		foreach (IProperty property in val.Properties)
		{
			if (property is ClaProperty { IsUnresolvedDef: false } claProperty)
			{
				if (claProperty.Getter == null && claProperty.CanGet)
				{
					arrayList.Add((object)new DefaultProperty("GET_" + claProperty.Name, claProperty.ReturnType, (ModifierEnum)0, DomRegion.Empty, DomRegion.Empty, claProperty.DeclaringType));
				}
				if (claProperty.Setter == null && claProperty.CanSet)
				{
					arrayList.Add((object)new DefaultProperty("SET_" + claProperty.Name, claProperty.ReturnType, (ModifierEnum)0, DomRegion.Empty, DomRegion.Empty, claProperty.DeclaringType));
				}
			}
		}
		ArrayList arrayList2 = new ArrayList();
		AddInterfaceNames(arrayList2, val);
		arrayList.AddRange(arrayList2);
		return arrayList;
	}

	private static void AddInterfaceNames(ArrayList res, IClass c)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		foreach (IReturnType baseType in c.BaseTypes)
		{
			IClass underlyingClass = baseType.GetUnderlyingClass();
			if (underlyingClass == null || (int)underlyingClass.ClassType != 2)
			{
				continue;
			}
			bool flag = false;
			foreach (IClass re in res)
			{
				IClass val = re;
				if (val.FullyQualifiedName == underlyingClass.FullyQualifiedName && val.TypeParameters.Count == underlyingClass.TypeParameters.Count)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				res.Add(underlyingClass);
			}
			AddInterfaceNames(res, underlyingClass);
		}
	}

	private ArrayList GetFromInterface()
	{
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Expected O, but got Unknown
		ClaClass claClass = (ClaClass)(object)((ResolveResult)this).CallingClass;
		IClass val = (IClass)(((object)cc) ?? ((object)claClass));
		ArrayList arrayList = new ArrayList();
		foreach (IMethod method in @interface.Methods)
		{
			bool flag = true;
			foreach (IMethod method2 in val.Methods)
			{
				if (method2 is ClaMethod && ((ClaMethod)(object)method2).CompareTo(method, fullName: false) == 0 && @interface.DotNetName.Equals(((ClaMethod)(object)method2).InterfaceImplementation, StringComparison.InvariantCultureIgnoreCase))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				arrayList.Add(method);
			}
		}
		foreach (IProperty property in @interface.Properties)
		{
			bool flag2 = true;
			bool flag3 = true;
			foreach (IProperty property2 in val.Properties)
			{
				if (property2 is ClaProperty && ((ClaProperty)(object)property2).CompareTo(property, fullName: false) == 0 && @interface.DotNetName.Equals(((ClaProperty)(object)property2).InterfaceImplementation, StringComparison.InvariantCultureIgnoreCase))
				{
					ClaProperty claProperty = (ClaProperty)(object)property2;
					if (claProperty.Getter != null)
					{
						flag3 = false;
					}
					if (claProperty.Setter != null)
					{
						flag2 = false;
					}
					break;
				}
			}
			if (flag3 && property.CanGet)
			{
				arrayList.Add((object)new DefaultProperty("GET_" + ((IMember)property).Name, ((IMember)property).ReturnType, (ModifierEnum)0, DomRegion.Empty, DomRegion.Empty, ((IDecoration)property).DeclaringType));
			}
			if (flag2 && property.CanSet)
			{
				arrayList.Add((object)new DefaultProperty("SET_" + ((IMember)property).Name, ((IMember)property).ReturnType, (ModifierEnum)0, DomRegion.Empty, DomRegion.Empty, ((IDecoration)property).DeclaringType));
			}
		}
		return arrayList;
	}
}
