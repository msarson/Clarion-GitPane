using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using SoftVelocity.Common.Parser;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public static class ClaCtrlSpaceResolveHelper
{
	private static void AddTypeParametersForCtrlSpace(ArrayList result, IEnumerable<ITypeParameter> typeParameters)
	{
		foreach (ITypeParameter typeParameter in typeParameters)
		{
			DefaultClass dummyClassForTypeParameter = DefaultTypeParameter.GetDummyClassForTypeParameter(typeParameter);
			if (typeParameter.Method != null)
			{
				((AbstractDecoration)dummyClassForTypeParameter).Documentation = "Type parameter of " + ((IMember)typeParameter.Method).Name;
			}
			else
			{
				((AbstractDecoration)dummyClassForTypeParameter).Documentation = "Type parameter of " + typeParameter.Class.Name;
			}
			result.Add(dummyClassForTypeParameter);
		}
	}

	public static void AddContentsFromCalling(ArrayList result, IClass callingClass, IMember callingMember)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		IMethodOrProperty val = (IMethodOrProperty)(object)((callingMember is IMethodOrProperty) ? callingMember : null);
		if (val != null)
		{
			foreach (IParameter parameter in val.Parameters)
			{
				if (!string.IsNullOrEmpty(parameter.Name))
				{
					if (parameter is ClaParameter && ((ClaParameter)(object)parameter).ReturnType is ClaReturnType)
					{
						ClaParameter claParameter = (ClaParameter)(object)parameter;
						result.Add(new ClaParameterField((ClaReturnType)(object)claParameter.ReturnType, claParameter.Name, claParameter.ClaRegion, callingClass));
					}
					else
					{
						result.Add((object)new ParameterField(parameter.ReturnType, parameter.Name, ((IMember)val).Region, callingClass));
					}
				}
			}
			if (callingMember is IMethod)
			{
				AddTypeParametersForCtrlSpace(result, ((IMethod)callingMember).TypeParameters);
				if (callingMember is ClaMethod claMethod)
				{
					AddLocals(result, claMethod);
					if (claMethod is ClaRoutine)
					{
						AddLocals(result, ((ClaRoutine)claMethod).DeclaringMethod);
					}
				}
			}
		}
		if (callingClass != null)
		{
			AddTypeParametersForCtrlSpace(result, callingClass.TypeParameters);
			if (callingClass is ClaLocalClass)
			{
				AddLocals(result, ((ClaLocalClass)(object)callingClass).DeclaringMethod);
			}
		}
	}

	public static void AddEquates(ArrayList result, ClaCompilationUnit cu)
	{
		if (cu == null)
		{
			return;
		}
		IClass globalClass = (IClass)(object)cu.GlobalClass;
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		if (!cu.IsProgram)
		{
			foreach (CEquateInfo value in cu.MemberEquates.Values)
			{
				if (!string.IsNullOrEmpty(value.Name) && !dictionary.ContainsKey(value.Name))
				{
					ClaEquate claEquate = new ClaEquate(value.Name, ClaDomRegion.Empty, globalClass);
					claEquate.SetDeclarationText(value.Text, cutLabel: true);
					result.Add(claEquate);
					dictionary.Add(value.Name, string.Empty);
				}
			}
		}
		foreach (CEquateInfo value2 in cu.ProgramEquates.Values)
		{
			if (!string.IsNullOrEmpty(value2.Name) && !dictionary.ContainsKey(value2.Name))
			{
				ClaEquate claEquate2 = new ClaEquate(value2.Name, ClaDomRegion.Empty, globalClass);
				claEquate2.SetDeclarationText(value2.Text, cutLabel: true);
				result.Add(claEquate2);
				dictionary.Add(value2.Name, string.Empty);
			}
		}
	}

	public static void AddGlobalContents(ArrayList result, ClaCompilationUnit cu, bool inCode)
	{
		if (cu == null)
		{
			return;
		}
		AddGlobals(result, cu.GlobalClass, cu, cu.FileName, inCode);
		CommonClarionProject commonClarionProject = ((cu.ProjectContent != null) ? (cu.ProjectContent.Project as CommonClarionProject) : null);
		if (commonClarionProject != null)
		{
			ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(commonClarionProject.ProgramFileName);
			if (parseInformationIfExist != null && parseInformationIfExist.BestCompilationUnit is ClaCompilationUnit { GlobalClass: { } globalClass } claCompilationUnit && globalClass.GetCompoundClass() != cu.GlobalClass.GetCompoundClass())
			{
				AddGlobals(result, globalClass, claCompilationUnit, cu.FileName, inCode);
			}
		}
	}

	public static void AddGlobals(ArrayList result, ClaGlobalClass globalClass, ClaCompilationUnit cu, string curFileName, bool inCode)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		ArrayList arrayList = new ArrayList();
		IClass compoundClass = globalClass.GetCompoundClass();
		foreach (IMethod method in compoundClass.Methods)
		{
			if (((IMember)method).Name.IndexOf(' ') == -1)
			{
				arrayList.Add(method);
			}
		}
		arrayList.AddRange(compoundClass.Fields);
		foreach (IMember item in arrayList)
		{
			IMember val = item;
			if (((IDecoration)val).IsAccessible((IClass)null, true))
			{
				result.Add(val);
				if (val is ClaField)
				{
					AddFEQs(result, (ClaField)(object)val);
				}
			}
			else if (val is ClaAbstractMember { IsPrivate: not false, ClaRegion: var claRegion } && claRegion.FileName.Equals(curFileName, StringComparison.InvariantCultureIgnoreCase))
			{
				result.Add(val);
				if (val is ClaField)
				{
					AddFEQs(result, (ClaField)(object)val);
				}
			}
		}
		foreach (ClaClass @class in cu.Classes)
		{
			if (@class.IsAccessible(null, isClassInInheritanceTree: true))
			{
				result.Add(@class);
			}
			else if (@class.IsPrivate && @class.ClaRegion.FileName.Equals(curFileName, StringComparison.InvariantCultureIgnoreCase))
			{
				result.Add(@class);
			}
		}
	}

	private static void AddLocals(ArrayList result, ClaMethod claM)
	{
		foreach (IClass localType in claM.LocalTypes)
		{
			result.Add(localType);
		}
		foreach (IMethod localMethod in claM.LocalMethods)
		{
			result.Add(localMethod);
		}
		foreach (IField localVariable in claM.LocalVariables)
		{
			result.Add(localVariable);
			if (localVariable is ClaField)
			{
				AddFEQs(result, (ClaField)(object)localVariable);
			}
		}
	}

	private static void AddFEQs(ArrayList result, ClaField claF)
	{
		if (claF.FEQList == null)
		{
			return;
		}
		foreach (ClaEquate fEQ in claF.FEQList)
		{
			result.Add(fEQ);
		}
	}

	public static void AddImportedNamespaceContents(ArrayList result, ICompilationUnit cu, IClass callingClass)
	{
		if (cu == null)
		{
			return;
		}
		IProjectContent projectContent = cu.ProjectContent;
		ArrayList arrayList = new ArrayList();
		projectContent.AddNamespaceContents(arrayList, "", projectContent.Language, true);
		foreach (object item in arrayList)
		{
			if (!(item is ClaGlobalClass))
			{
				result.Add(item);
			}
		}
		foreach (IUsing @using in cu.Usings)
		{
			AddUsing(result, @using, projectContent);
		}
		AddUsing(result, projectContent.DefaultImports, projectContent);
		if (callingClass == null)
		{
			return;
		}
		string[] array = callingClass.Namespace.Split('.');
		for (int i = 1; i <= array.Length; i++)
		{
			foreach (object namespaceContent in projectContent.GetNamespaceContents(string.Join(".", array, 0, i)))
			{
				if (!result.Contains(namespaceContent))
				{
					result.Add(namespaceContent);
				}
			}
		}
		IClass val = callingClass;
		do
		{
			foreach (IClass accessibleType in val.GetAccessibleTypes(val))
			{
				if (!result.Contains(accessibleType))
				{
					result.Add(accessibleType);
				}
			}
			val = ((IDecoration)val).DeclaringType;
		}
		while (val != null);
	}

	public static void AddUsing(ArrayList result, IUsing u, IProjectContent projectContent)
	{
		if (u == null || projectContent == null)
		{
			return;
		}
		foreach (string @using in u.Usings)
		{
			foreach (object namespaceContent in projectContent.GetNamespaceContents(@using))
			{
				if (!(namespaceContent is string))
				{
					result.Add(namespaceContent);
				}
			}
		}
		if (!u.HasAliases)
		{
			return;
		}
		foreach (string key in u.Aliases.Keys)
		{
			result.Add(key);
		}
	}

	public static void AddNetRTLContents(ArrayList result, IProjectContent pc)
	{
		if (pc == null)
		{
			return;
		}
		IClass val = pc.GetClass("Clarion.RTLProcs");
		if (val == null)
		{
			return;
		}
		foreach (IMethod method in val.Methods)
		{
			if (((IMember)method).Name.StartsWith("_"))
			{
				continue;
			}
			bool flag = true;
			foreach (IAttribute attribute in ((IDecoration)method).Attributes)
			{
				if (attribute.AttributeType.FullyQualifiedName == "Clarion.C7Attribute")
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				result.Add(method);
			}
		}
	}

	public static IClass GetPrimitiveClass(IProjectContent pc, string systemType, string newName)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		IClass val = pc.GetClass(systemType);
		if (val != null)
		{
			DefaultClass val2 = new DefaultClass(val.CompilationUnit, newName);
			val2.ClassType = val.ClassType;
			((AbstractDecoration)val2).Modifiers = ((IDecoration)val).Modifiers;
			((AbstractDecoration)val2).Documentation = ((IDecoration)val).Documentation;
			val2.BaseTypes.AddRange(val.BaseTypes);
			val2.Methods.AddRange(val.Methods);
			val2.Fields.AddRange(val.Fields);
			val2.Properties.AddRange(val.Properties);
			val2.Events.AddRange(val.Events);
			return (IClass)(object)val2;
		}
		return null;
	}
}
