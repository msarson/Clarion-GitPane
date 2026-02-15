using System;
using System.Collections;
using System.Collections.Generic;
using Clarion;
using Clarion.Core.Redirection;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common.Parser;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public abstract class CommonClarionResolver : AbstractClarionResolver
{
	protected ClaCompilationUnit cu;

	protected IProjectContent pc;

	protected IClass callingClass;

	protected IMember callingMember;

	private CodeCompletionTagInfo tagInfo;

	private int caretLine;

	private int caretColumn;

	private string fileContent;

	public abstract bool IsWin { get; }

	public override ClaCompilationUnit CompilationUnit => cu;

	public override IProjectContent ProjectContent => pc;

	public override IClass CallingClass => callingClass;

	public override IMember CallingMember => callingMember;

	public override int CaretLine => caretLine;

	public override int CaretColumn => caretColumn;

	public string FileContent => fileContent;

	protected virtual object ObjFromLine(int line)
	{
		cu.Line2Decl.TryGetValue(line, out var value);
		return value;
	}

	private Expression ParseExpression(string expression)
	{
		CompilerOptions compilerOptions = new CompilerOptions();
		compilerOptions.c7mode = IsWin;
		compilerOptions.debug = true;
		compilerOptions.outFileName = "Dummy";
		ref object redFile = ref compilerOptions.redFile;
		object project = cu.ProjectContent.Project;
		redFile = CommonClarionProject.CurrentRedirectionFile((IProject)((project is IProject) ? project : null), cu.IsWin);
		compilerOptions.redType = typeof(RedirectionFile);
		return ClarionParser.ParseExpression(compilerOptions, expression);
	}

	private static string GetFixedExpression(ExpressionResult expressionResult)
	{
		string text = expressionResult.Expression;
		if (text == null)
		{
			text = string.Empty;
		}
		return text.TrimStart();
	}

	protected virtual bool Initialize(string fileName, string fileContent, int caretLineNumber, int caretColumnNumber, CodeCompletionTagInfo tagInfo)
	{
		caretLine = caretLineNumber;
		caretColumn = caretColumnNumber;
		this.fileContent = fileContent;
		this.tagInfo = tagInfo ?? new CodeCompletionTagInfo();
		ParseInformation parseInformation = ParserService.GetParseInformation(fileName);
		if (parseInformation == null)
		{
			return false;
		}
		cu = parseInformation.MostRecentCompilationUnit as ClaCompilationUnit;
		if (cu == null)
		{
			return false;
		}
		pc = cu.ProjectContent;
		InitClassAndMethod(fileName);
		if (callingClass == null)
		{
			return false;
		}
		return true;
	}

	protected virtual void InitClassAndMethod(string fileName)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		callingClass = null;
		callingMember = null;
		object obj = cu.FindNearestObject(caretLine, caretColumn);
		if (obj is IClass)
		{
			callingClass = (IClass)obj;
		}
		else if (obj is IMember)
		{
			callingMember = (IMember)obj;
			callingClass = ((IDecoration)callingMember).DeclaringType;
		}
		if (callingClass == null)
		{
			callingClass = cu.GetInnermostClass(caretLine, caretColumn);
		}
		if (callingClass == null)
		{
			callingClass = (IClass)(object)cu.GlobalClass;
		}
	}

	public override ResolveResult Resolve(ExpressionResult expressionResult, int caretLineNum, int caretColumnNum, string fName, string fContent)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		string fixedExpression = GetFixedExpression(expressionResult);
		if (!Initialize(fName, fContent, caretLineNum, caretColumnNum, expressionResult.Tag as CodeCompletionTagInfo))
		{
			return null;
		}
		if (tagInfo.IsRoutine)
		{
			if (callingMember is ClaMethod)
			{
				return (ResolveResult)(object)new ClaRoutinesResolveResult((ClaMethod)(object)callingMember);
			}
			return null;
		}
		Expression expression = ParseExpression(fixedExpression);
		if (expressionResult.Context.IsObjectCreation)
		{
			for (Expression expression2 = expression; expression2 != null; expression2 = (expression2 as FieldReferenceExpression).TargetObject)
			{
				if (expression2 is IdentifierExpression)
				{
					return ResolveInternal(expression, ExpressionContext.Type);
				}
				if (!(expression2 is FieldReferenceExpression))
				{
					break;
				}
			}
			expression = ParseExpression("new " + fixedExpression);
		}
		if (expression == null)
		{
			return null;
		}
		if (expressionResult.Context.IsAttributeContext)
		{
			return ResolveAttribute(expression);
		}
		object obj;
		if (expression is IdentifierExpression && (obj = ObjFromLine(caretLine)) != null)
		{
			if (obj is IClass && caretColumn <= ((IClass)obj).Name.Length)
			{
				return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, ((IClass)obj).DefaultReturnType));
			}
			if (obj is IMember && caretColumn <= ((IMember)obj).Name.Length)
			{
				if (obj is ClaLocalVariableField)
				{
					return (ResolveResult)(object)new ClaLocalResolveResult(callingMember, (IField)obj);
				}
				return CreateMemberResolveResult((IMember)obj);
			}
		}
		return ResolveInternal(expression, expressionResult.Context);
	}

	private string GetAttributeName(Expression expr)
	{
		if (expr is IdentifierExpression)
		{
			return (expr as IdentifierExpression).Identifier;
		}
		if (expr is FieldReferenceExpression)
		{
			ClaTypeVisitor visitor = new ClaTypeVisitor(this);
			FieldReferenceExpression fieldReferenceExpression = (FieldReferenceExpression)expr;
			object obj = fieldReferenceExpression.TargetObject.AcceptVisitor(visitor, null);
			IReturnType val = (IReturnType)((obj is IReturnType) ? obj : null);
			if (val is ClaTypeVisitor.NamespaceReturnType)
			{
				return val.FullyQualifiedName + "." + fieldReferenceExpression.FieldName;
			}
		}
		return null;
	}

	private IClass GetAttribute(string name)
	{
		if (name == null)
		{
			return null;
		}
		IClass val = SearchClass(name);
		if (val != null && val.IsTypeInInheritanceTree(val.ProjectContent.SystemTypes.Attribute.GetUnderlyingClass()))
		{
			return val;
		}
		return SearchClass(name + "Attribute");
	}

	protected virtual ResolveResult ResolveAttribute(Expression expr)
	{
		string attributeName = GetAttributeName(expr);
		IClass attribute = GetAttribute(attributeName);
		if (attribute != null)
		{
			return (ResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, attribute);
		}
		if (expr is InvocationExpression)
		{
			InvocationExpression invocationExpression = (InvocationExpression)expr;
			attributeName = GetAttributeName(invocationExpression.TargetObject);
			attribute = GetAttribute(attributeName);
			if (attribute != null)
			{
				List<IMethod> list = new List<IMethod>();
				foreach (IMethod method in attribute.Methods)
				{
					if (method.IsConstructor && !((IDecoration)method).IsStatic)
					{
						list.Add(method);
					}
				}
				ClaTypeVisitor claTypeVisitor = new ClaTypeVisitor(this);
				return CreateMemberResolveResult((IMember)(object)claTypeVisitor.FindOverload(list, null, invocationExpression.Arguments, null));
			}
		}
		return null;
	}

	public ResolveResult ResolveInternal(Expression expr, ExpressionContext context)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Invalid comparison between Unknown and I4
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Invalid comparison between Unknown and I4
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		ClaTypeVisitor claTypeVisitor = new ClaTypeVisitor(this);
		if (expr is PrimitiveExpression)
		{
			return null;
		}
		IReturnType resolvedType;
		if (expr is InvocationExpression)
		{
			if (tagInfo.IsPre)
			{
				return null;
			}
			IMethodOrProperty method = claTypeVisitor.GetMethod(expr as InvocationExpression);
			if (method != null)
			{
				return CreateMemberResolveResult((IMember)(object)method);
			}
			ResolveResult val = ResolveInternal((expr as InvocationExpression).TargetObject, ExpressionContext.Default);
			if (val == null)
			{
				return null;
			}
			resolvedType = val.ResolvedType;
			if (resolvedType == null)
			{
				return null;
			}
			IClass underlyingClass = resolvedType.GetUnderlyingClass();
			if (underlyingClass == null || (int)underlyingClass.ClassType != 4)
			{
				return null;
			}
			method = (IMethodOrProperty)(object)underlyingClass.Methods.Find((IMethod innerMethod) => ((IMember)innerMethod).Name == "Invoke");
			if (method != null)
			{
				val.ResolvedType = ((IMember)method).ReturnType;
			}
			return val;
		}
		if (expr is IndexedExpression)
		{
			if (tagInfo.IsPre)
			{
				return null;
			}
			ResolveResult val2 = CreateMemberResolveResult((IMember)(object)claTypeVisitor.GetIndexer(expr as IndexedExpression));
			if (val2 == null)
			{
				val2 = ResolveInternal(((IndexedExpression)expr).Expr, context);
				if (val2 != null)
				{
					if (val2 is LocalResolveResult)
					{
						return val2;
					}
					if (val2.ResolvedType == null)
					{
						return null;
					}
					if (!(val2.ResolvedType.GetUnderlyingClass() is ClaClass { Dimensions: not 0, IsTypeOnly: false }) || !(val2 is MixedResolveResult))
					{
						return null;
					}
					MixedResolveResult val3 = (MixedResolveResult)val2;
					val2 = null;
					if (!tagInfo.IsDot)
					{
						return (ResolveResult)(object)val3;
					}
					foreach (ResolveResult result in val3.Results)
					{
						if (!(result is TypeResolveResult))
						{
							return result;
						}
					}
				}
			}
			return val2;
		}
		if (expr is FieldReferenceExpression)
		{
			FieldReferenceExpression fieldReferenceExpression = (FieldReferenceExpression)expr;
			object obj = fieldReferenceExpression.TargetObject.AcceptVisitor(claTypeVisitor, null);
			resolvedType = (IReturnType)((obj is IReturnType) ? obj : null);
			if (resolvedType != null)
			{
				ResolveResult val4 = ResolveMemberReferenceExpression(resolvedType, fieldReferenceExpression);
				if (val4 != null)
				{
					if (tagInfo.IsLabelExpression && val4.ResolvedType != null)
					{
						IClass underlyingClass2 = val4.ResolvedType.GetUnderlyingClass();
						if (underlyingClass2 != null && (int)underlyingClass2.ClassType == 2)
						{
							ClaClass claClass2 = resolvedType.GetUnderlyingClass() as ClaClass;
							IClass underlyingClass3 = resolvedType.GetUnderlyingClass();
							CompoundClass val5 = (CompoundClass)(object)((underlyingClass3 is CompoundClass) ? underlyingClass3 : null);
							if (claClass2 == null && val5 != null)
							{
								foreach (IClass part in val5.GetParts())
								{
									if (part is ClaClass && cu.FileName.Equals(((ClaClass)(object)part).ClaRegion.FileName, StringComparison.InvariantCultureIgnoreCase))
									{
										claClass2 = (ClaClass)(object)part;
										break;
									}
								}
							}
							if (claClass2 != null && !(claClass2 is ClaGlobalClass) && cu.FileName.Equals(claClass2.ClaRegion.FileName, StringComparison.InvariantCultureIgnoreCase))
							{
								return (ResolveResult)(object)new ClaProcedureDefResolveResult(claClass2, val5, underlyingClass2);
							}
						}
					}
					return val4;
				}
			}
		}
		else if (expr is IdentifierExpression)
		{
			ResolveResult val6 = ResolveIdentifier((IdentifierExpression)expr, onlyGlobalAndTypes: true, context == ClaExpressionContext.LIKE);
			if (val6 != null)
			{
				if (tagInfo.IsLabelExpression && (val6 is TypeResolveResult || val6 is MixedResolveResult))
				{
					ClaClass claClass3 = ((val6 is TypeResolveResult) ? (((TypeResolveResult)val6).ResolvedClass as ClaClass) : (((MixedResolveResult)val6).TypeResult.ResolvedClass as ClaClass));
					CompoundClass val7 = (CompoundClass)((val6 is TypeResolveResult) ? /*isinst with value type is only supported in some contexts*/: /*isinst with value type is only supported in some contexts*/);
					if (claClass3 == null && val7 != null)
					{
						foreach (IClass part2 in val7.GetParts())
						{
							if (part2 is ClaClass && cu.FileName.Equals(((ClaClass)(object)part2).ClaRegion.FileName, StringComparison.InvariantCultureIgnoreCase))
							{
								claClass3 = (ClaClass)(object)part2;
								break;
							}
						}
					}
					if (claClass3 != null && !(claClass3 is ClaGlobalClass) && cu.FileName.Equals(claClass3.ClaRegion.FileName, StringComparison.InvariantCultureIgnoreCase))
					{
						return (ResolveResult)(object)new ClaProcedureDefResolveResult(claClass3, val7, null);
					}
				}
				return val6;
			}
		}
		else
		{
			if (expr is TypeReferenceExpression)
			{
				return ResolveTypeReference(((TypeReferenceExpression)expr).TypeReference);
			}
			if (expr is ThisReferenceExpression)
			{
				if (callingClass is ClaGlobalClass)
				{
					return null;
				}
				if (!tagInfo.IsDot)
				{
					if (callingClass != null)
					{
						return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, callingClass.DefaultReturnType, callingClass));
					}
					return null;
				}
			}
			else if (expr is BaseReferenceExpression)
			{
				if (callingClass is ClaGlobalClass)
				{
					return null;
				}
				if (!tagInfo.IsDot)
				{
					if (callingClass != null)
					{
						IClass baseClass = callingClass.BaseClass;
						if (baseClass != null)
						{
							return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, baseClass.DefaultReturnType, baseClass));
						}
					}
					return null;
				}
			}
		}
		if (tagInfo.IsPre)
		{
			return null;
		}
		object obj2 = expr.AcceptVisitor(claTypeVisitor, null);
		resolvedType = (IReturnType)((obj2 is IReturnType) ? obj2 : null);
		if (resolvedType == null || resolvedType.FullyQualifiedName == "")
		{
			return null;
		}
		if (expr is ObjectCreateExpression)
		{
			List<IMethod> list = new List<IMethod>();
			foreach (IMethod method2 in resolvedType.GetMethods())
			{
				if (method2.IsConstructor && !((IDecoration)method2).IsStatic)
				{
					list.Add(method2);
				}
			}
			if (list.Count == 0)
			{
				IClass underlyingClass4 = resolvedType.GetUnderlyingClass();
				if (underlyingClass4 != null)
				{
					return CreateMemberResolveResult((IMember)(object)ClaConstructor.CreateDefault(underlyingClass4));
				}
			}
			IReturnType[] array = null;
			if (resolvedType.IsConstructedReturnType)
			{
				array = (IReturnType[])(object)new IReturnType[resolvedType.CastToConstructedReturnType().TypeArguments.Count];
				resolvedType.CastToConstructedReturnType().TypeArguments.CopyTo(array, 0);
			}
			ResolveResult val8 = CreateMemberResolveResult((IMember)(object)claTypeVisitor.FindOverload(list, array, ((ObjectCreateExpression)expr).Parameters, null));
			if (val8 != null)
			{
				val8.ResolvedType = resolvedType;
			}
			return val8;
		}
		return (ResolveResult)(object)new ClaResolveResult(callingClass, callingMember, resolvedType);
	}

	public override ResolveResult ResolveTypeReference(TypeReference typeRef)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		IReturnType val = (IReturnType)(object)ClaTypeVisitor.CreateReturnType(typeRef, this);
		if (val != null)
		{
			if (val is ClaTypeVisitor.NamespaceReturnType)
			{
				return (ResolveResult)new NamespaceResolveResult(callingClass, callingMember, val.FullyQualifiedName);
			}
			IClass underlyingClass = val.GetUnderlyingClass();
			if (underlyingClass != null)
			{
				return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, val, underlyingClass));
			}
		}
		return null;
	}

	public override ResolveResult ResolveMemberReferenceExpression(IReturnType type, FieldReferenceExpression fieldReferenceExpression)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Invalid comparison between Unknown and I4
		IClass val2;
		if (type is ClaTypeVisitor.NamespaceReturnType)
		{
			string fullyQualifiedName = type.FullyQualifiedName;
			string text = ((!string.IsNullOrEmpty(fullyQualifiedName)) ? (fullyQualifiedName + "." + fieldReferenceExpression.FieldName) : fieldReferenceExpression.FieldName);
			if (pc.NamespaceExists(text))
			{
				return (ResolveResult)new NamespaceResolveResult(callingClass, callingMember, text);
			}
			if (fieldReferenceExpression.GenericArgsExists && fieldReferenceExpression.GenericArgs.Count > 0)
			{
				TypeReference typeReference = new TypeReference(text);
				typeReference.GenericArgs = new GenericArguments();
				typeReference.GenericArgs.AddRange(fieldReferenceExpression.GenericArgs);
				typeReference.IsGeneric = true;
				ResolveResult val = ResolveTypeReference(typeReference);
				if (val != null)
				{
					return val;
				}
			}
			val2 = GetClass(text);
			if (val2 != null)
			{
				return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, val2));
			}
			if (!string.IsNullOrEmpty(fullyQualifiedName))
			{
				val2 = GetClass(fullyQualifiedName + "." + ClaGlobalClass.globalClassName);
				if (val2 != null)
				{
					return ResolveIdentifierInClass(val2, fieldReferenceExpression.FieldName);
				}
			}
			return null;
		}
		IMember member = GetMember(type, fieldReferenceExpression.FieldName);
		if (member != null)
		{
			return CreateMemberResolveResult(member);
		}
		val2 = type.GetUnderlyingClass();
		if (val2 != null)
		{
			foreach (IClass item in val2.ClassInheritanceTree)
			{
				List<IClass> innerClasses = item.InnerClasses;
				if (innerClasses != null)
				{
					foreach (IClass item2 in innerClasses)
					{
						if (IsSameName(item2.Name, fieldReferenceExpression.FieldName))
						{
							return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, item2));
						}
					}
				}
				if ((int)item.ClassType == 2 && IsSameName(item.Name, fieldReferenceExpression.FieldName) && item.TypeParameters.Count == (fieldReferenceExpression.GenericArgsExists ? fieldReferenceExpression.GenericArgs.Count : 0))
				{
					return (ResolveResult)(object)new ClaResolveResult(callingClass, callingMember, item.DefaultReturnType);
				}
			}
			if (val2 is ClaClass && ((ClaClass)(object)val2).ClarionType == ClarionType.FILE)
			{
				foreach (IClass innerClass in val2.InnerClasses)
				{
					if (!(innerClass is ClaClass) || ((ClaClass)(object)innerClass).ClarionType != ClarionType.RECORD)
					{
						continue;
					}
					foreach (IClass innerClass2 in innerClass.InnerClasses)
					{
						if (IsSameName(innerClass2.Name, fieldReferenceExpression.FieldName))
						{
							return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, innerClass2));
						}
					}
					break;
				}
			}
		}
		if (callingMember is ClaAbstractMember && IsSameName(callingMember.Name, fieldReferenceExpression.FieldName))
		{
			ClaAbstractMember claAbstractMember = (ClaAbstractMember)(object)callingMember;
			if (!claAbstractMember.ClaBodyRegion.IsEmpty)
			{
				DomRegion val3 = default(DomRegion);
				((DomRegion)(ref val3))._002Ector(claAbstractMember.ClaBodyRegion.DeclBeginLine, claAbstractMember.ClaBodyRegion.DeclBeginColumn, claAbstractMember.ClaBodyRegion.BeginLine, claAbstractMember.ClaBodyRegion.BeginColumn);
				if (((DomRegion)(ref val3)).IsInside(caretLine, caretColumn))
				{
					return CreateMemberResolveResult((IMember)(object)claAbstractMember);
				}
			}
		}
		foreach (IMethod method in type.GetMethods())
		{
			if (IsSameName(fieldReferenceExpression.FieldName, ((IMember)method).Name) && ((IMethodOrProperty)method).Parameters.Count == 0)
			{
				return CreateMemberResolveResult((IMember)(object)method);
			}
		}
		return ResolveMethod(type, fieldReferenceExpression.FieldName);
	}

	protected virtual ResolveResult ResolveIdentifier(IdentifierExpression identifierExpr, bool onlyGlobalAndTypes, bool isInLike)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		ResolveResult val = null;
		if (!tagInfo.IsPre)
		{
			val = ResolveIdentifierInternal(identifierExpr, onlyGlobalAndTypes, isInLike);
			if (val is TypeResolveResult)
			{
				return ProcessTypeOnly((TypeResolveResult)val);
			}
			if (val != null)
			{
				return val;
			}
			string identifier = identifierExpr.Identifier;
			IReturnType val2 = SearchType(identifier);
			if (val2 != null)
			{
				return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, val2));
			}
			if (callingClass != null)
			{
				if (callingMember is IMethod)
				{
					IMember obj = callingMember;
					foreach (ITypeParameter typeParameter in ((IMethod)((obj is IMethod) ? obj : null)).TypeParameters)
					{
						if (IsSameName(identifier, typeParameter.Name))
						{
							return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, (IReturnType)new GenericReturnType(typeParameter)));
						}
					}
				}
				foreach (ITypeParameter typeParameter2 in callingClass.TypeParameters)
				{
					if (IsSameName(identifier, typeParameter2.Name))
					{
						return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, (IReturnType)new GenericReturnType(typeParameter2)));
					}
				}
			}
		}
		val = ResolveEquateIdentifier(identifierExpr.Identifier);
		if (val == null)
		{
			val = ResolveIdentifierWithPre(identifierExpr.Identifier);
		}
		if (val == null && isInLike)
		{
			val = ResolveIdentifierInMember(callingMember, identifierExpr.Identifier);
		}
		if (val == null)
		{
			return (ResolveResult)(object)new UnknownIdentifierResolveResult(CallingClass, CallingMember, identifierExpr.Identifier, identifierExpr.GenericArgsExists ? identifierExpr.GenericArgs.Count : 0);
		}
		return val;
	}

	private static ResolveResult ProcessTypeOnly(TypeResolveResult result)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if (result.ResolvedClass is ClaClass)
		{
			ClaClass claClass = (ClaClass)(object)result.ResolvedClass;
			if (!claClass.IsTypeOnly)
			{
				ResolveResult val = (ResolveResult)(object)new ClaResolveResult(((ResolveResult)result).CallingClass, ((ResolveResult)result).CallingMember, result.ResolvedClass.DefaultReturnType);
				return (ResolveResult)new MixedResolveResult((ResolveResult)(object)result, val);
			}
		}
		return (ResolveResult)(object)result;
	}

	protected virtual ResolveResult ResolveIdentifierInMember(IMember member, string identifier)
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		if (member == null)
		{
			return null;
		}
		if (member is ClaMethod)
		{
			foreach (IField localVariable in ((ClaMethod)(object)member).LocalVariables)
			{
				if (IsSameName(((IMember)localVariable).Name, identifier))
				{
					return (ResolveResult)(object)new ClaLocalResolveResult(member, localVariable);
				}
			}
			foreach (ClaRoutine routine in ((ClaMethod)(object)member).Routines)
			{
				if (IsSameName(routine.Name, identifier))
				{
					return (ResolveResult)new MemberResolveResult(callingClass, member, (IMember)(object)routine);
				}
			}
		}
		IParameter val = SearchMethodParameter(member, identifier);
		if (val is ClaParameter)
		{
			IField field = (IField)(object)new ClaParameterField((ClaReturnType)(object)val.ReturnType, val.Name, ((ClaParameter)(object)val).ClaRegion, callingClass);
			return (ResolveResult)(object)new ClaLocalResolveResult(member, field);
		}
		if (IsSameName(identifier, "value"))
		{
			IProperty val2 = (IProperty)(object)((member is IProperty) ? member : null);
			if (val2 is ClaProperty)
			{
				DomRegion setterRegion = val2.SetterRegion;
				if (((DomRegion)(ref setterRegion)).IsInside(caretLine, caretColumn))
				{
					IField field2 = (IField)(object)new ClaParameterField((ClaReturnType)(object)((IMember)val2).ReturnType, "value", ((ClaProperty)(object)val2).ClaRegion, callingClass);
					return (ResolveResult)(object)new ClaLocalResolveResult(member, field2);
				}
			}
		}
		ClaMethod claMethod = null;
		if (member is ClaRoutine)
		{
			claMethod = ((ClaRoutine)(object)member).DeclaringMethod;
		}
		else if (member is ClaLocalMethod)
		{
			claMethod = ((ClaLocalMethod)(object)member).DeclaringMethod;
		}
		if (claMethod != null)
		{
			return ResolveIdentifierInMember((IMember)(object)claMethod, identifier);
		}
		return null;
	}

	protected virtual ResolveResult ResolveIdentifierWithPre(string identifier)
	{
		int num = ((!tagInfo.IsPre) ? identifier.IndexOf(':') : identifier.Length);
		if (num == -1)
		{
			return null;
		}
		string name = identifier.Substring(0, num);
		IReturnType val = SearchTypeByPre(name);
		if (val != null)
		{
			if (tagInfo.IsPre)
			{
				return (ResolveResult)(object)new ClaPreTypeResolveResult(callingClass, callingMember, val);
			}
			if (num + 1 >= identifier.Length)
			{
				return null;
			}
			string text = identifier.Substring(num + 1);
			IMember member = GetMember(val, text);
			if (member != null)
			{
				return CreateMemberResolveResult(member);
			}
			ResolveResult val2 = ResolveMethod(val, text);
			if (val2 != null)
			{
				return val2;
			}
			if (val is ClaPreCollectionReturnType)
			{
				ClaPreCollectionReturnType claPreCollectionReturnType = (ClaPreCollectionReturnType)(object)val;
				foreach (IClass innerClass in claPreCollectionReturnType.GetInnerClasses())
				{
					if (IsSameName(innerClass.Name, text))
					{
						return ProcessTypeOnly((TypeResolveResult)(object)new ClaTypeResolveResult(callingClass, callingMember, innerClass));
					}
				}
			}
		}
		return null;
	}

	protected virtual ResolveResult ResolveEquateIdentifier(string identifier)
	{
		string key = identifier.ToUpperInvariant();
		if (cu.ProgramEquates != null && cu.ProgramEquates.Contains(key))
		{
			CEquateInfo cEquateInfo = (CEquateInfo)cu.ProgramEquates[key];
			CASTPosition pos = cEquateInfo.Pos;
			ClaDomRegion pos2 = ((pos != null) ? new ClaDomRegion(pos.Line, pos.Column, pos.Line, pos.Column, pos.File) : ClaDomRegion.Empty);
			return (ResolveResult)(object)new ClaEquateResolveResult(cu, callingClass, callingMember, cEquateInfo.Name, cEquateInfo.Text, pos2);
		}
		if (cu.MemberEquates != null && cu.MemberEquates.Contains(key))
		{
			CEquateInfo cEquateInfo2 = (CEquateInfo)cu.MemberEquates[key];
			CASTPosition pos3 = cEquateInfo2.Pos;
			ClaDomRegion pos4 = ((pos3 != null) ? new ClaDomRegion(pos3.Line, pos3.Column, pos3.Line, pos3.Column, pos3.File) : ClaDomRegion.Empty);
			return (ResolveResult)(object)new ClaEquateResolveResult(cu, callingClass, callingMember, cEquateInfo2.Name, cEquateInfo2.Text, pos4);
		}
		return null;
	}

	protected virtual ResolveResult ResolveIdentifierInternal(IdentifierExpression identifierExpr, bool onlyGlobalAndTypes, bool isInLike)
	{
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		if (identifierExpr.GenericArgsExists && identifierExpr.GenericArgs.Count > 0)
		{
			TypeReference typeReference = new TypeReference(identifierExpr.Identifier);
			typeReference.GenericArgs = new GenericArguments();
			typeReference.GenericArgs.AddRange(identifierExpr.GenericArgs);
			typeReference.IsGeneric = true;
			ResolveResult val = ResolveTypeReference(typeReference);
			if (val != null)
			{
				return val;
			}
		}
		string identifier = identifierExpr.Identifier;
		if (!isInLike)
		{
			ResolveResult val2 = ResolveIdentifierInMember(callingMember, identifier);
			if (val2 != null)
			{
				return val2;
			}
		}
		if (callingClass is ClaGlobalClass || (!(callingClass is ClaGlobalClass) && !onlyGlobalAndTypes))
		{
			ResolveResult val2 = ResolveIdentifierInClass(callingClass, identifier);
			if (val2 != null)
			{
				return val2;
			}
		}
		if (callingClass is ClaLocalClass)
		{
			ResolveResult val2 = ResolveIdentifierInMember((IMember)(object)((ClaLocalClass)(object)callingClass).DeclaringMethod, identifier);
			if (val2 != null)
			{
				return val2;
			}
		}
		if (cu.IsProgram)
		{
			if (!(callingClass is ClaGlobalClass))
			{
				ResolveResult val2 = ResolveIdentifierInClass((IClass)(object)cu.GlobalClass, identifier);
				if (val2 != null)
				{
					return val2;
				}
			}
		}
		else
		{
			if (!(callingClass is ClaGlobalClass))
			{
				ResolveResult val2 = ResolveIdentifierInClass((IClass)(object)cu.GlobalClass, identifier);
				if (val2 != null)
				{
					return val2;
				}
			}
			string text = ((pc.Project is CommonClarionProject) ? ((CommonClarionProject)pc.Project).ProgramFileName : null);
			if (!string.IsNullOrEmpty(text))
			{
				ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(text);
				if (parseInformationIfExist != null && parseInformationIfExist.MostRecentCompilationUnit is ClaCompilationUnit)
				{
					ClaCompilationUnit claCompilationUnit = (ClaCompilationUnit)(object)parseInformationIfExist.MostRecentCompilationUnit;
					ResolveResult val2 = ResolveIdentifierInClass((IClass)(object)claCompilationUnit.GlobalClass, identifier);
					if (val2 != null)
					{
						return val2;
					}
				}
			}
		}
		if (!cu.IsWin)
		{
			string text2 = SearchNamespace(identifier);
			if (text2 != null && text2.Length > 0)
			{
				return (ResolveResult)new NamespaceResolveResult(callingClass, callingMember, text2);
			}
			IReturnType val3 = SearchType("Clarion.RTLProcs");
			if (val3 != null)
			{
				ResolveResult val2 = ResolveIdentifierInClass(val3.GetUnderlyingClass(), identifier);
				if (val2 != null)
				{
					return val2;
				}
			}
		}
		return null;
	}

	protected virtual ResolveResult ResolveIdentifierInClass(IClass @class, string identifier)
	{
		if (@class != null)
		{
			IMember member = GetMember(@class.DefaultReturnType, identifier);
			if (member != null)
			{
				return CreateMemberResolveResult(member);
			}
			if (callingMember is ClaAbstractMember && IsSameName(callingMember.Name, identifier))
			{
				ClaAbstractMember claAbstractMember = (ClaAbstractMember)(object)callingMember;
				if (!claAbstractMember.ClaBodyRegion.IsEmpty)
				{
					DomRegion val = default(DomRegion);
					((DomRegion)(ref val))._002Ector(claAbstractMember.ClaBodyRegion.DeclBeginLine, claAbstractMember.ClaBodyRegion.DeclBeginColumn, claAbstractMember.ClaBodyRegion.BeginLine, claAbstractMember.ClaBodyRegion.BeginColumn);
					if (((DomRegion)(ref val)).IsInside(caretLine, caretColumn))
					{
						return CreateMemberResolveResult((IMember)(object)claAbstractMember);
					}
				}
				if (claAbstractMember.ClaRegion.IsInside(caretLine, caretColumn))
				{
					return CreateMemberResolveResult((IMember)(object)claAbstractMember);
				}
			}
			foreach (IMethod method in @class.GetCompoundClass().Methods)
			{
				if (IsSameName(identifier, ((IMember)method).Name) && ((IMethodOrProperty)method).Parameters.Count == 0)
				{
					return CreateMemberResolveResult((IMember)(object)method);
				}
			}
			ResolveResult val2 = ResolveMethod(@class.DefaultReturnType, identifier);
			if (val2 != null)
			{
				return val2;
			}
		}
		return null;
	}

	private ResolveResult CreateMemberResolveResult(IMember member)
	{
		if (member == null)
		{
			return null;
		}
		return (ResolveResult)(object)new ClaMemberResolveResult(callingClass, callingMember, member);
	}

	protected virtual ResolveResult ResolveMethod(IReturnType type, string identifier)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if (type == null)
		{
			return null;
		}
		foreach (IMethod method in type.GetMethods())
		{
			if (IsSameName(identifier, ((IMember)method).Name))
			{
				return (ResolveResult)new MethodResolveResult(callingClass, callingMember, type, identifier);
			}
		}
		return null;
	}

	public bool IsSameName(string name1, string name2)
	{
		return name1.Equals(name2, StringComparison.InvariantCultureIgnoreCase);
	}

	public string SearchNamespace(string name)
	{
		return pc.SearchNamespace(name, callingClass, (ICompilationUnit)(object)cu, caretLine, caretColumn);
	}

	public override IClass GetClass(string fullName)
	{
		return pc.GetClass(fullName);
	}

	public override IClass SearchClass(string name)
	{
		IReturnType val = SearchType(name);
		if (val == null)
		{
			return null;
		}
		return val.GetUnderlyingClass();
	}

	public virtual IReturnType SearchType(string name)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		SearchTypeResult val = pc.SearchType(new SearchTypeRequest(name, 0, callingClass, (ICompilationUnit)(object)cu, caretLine, caretColumn));
		return ((SearchTypeResult)(ref val)).Result;
	}

	public virtual IReturnType SearchTypeByPre(string name)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!(pc is IClarionProjectContent))
		{
			return null;
		}
		SearchTypeResult val = ((IClarionProjectContent)pc).SearchTypeByPre(new SearchTypeRequest(name, 0, callingClass, (ICompilationUnit)(object)cu, caretLine, caretColumn));
		return ((SearchTypeResult)(ref val)).Result;
	}

	public override List<IMethod> SearchMethod(string memberName)
	{
		List<IMethod> list = new List<IMethod>();
		IClass globalClass = (IClass)(object)cu.GlobalClass;
		if (globalClass != null)
		{
			list.AddRange(SearchMethod(globalClass.DefaultReturnType, memberName));
		}
		if (cu.IsProgram)
		{
			return list;
		}
		string text = ((pc.Project is CommonClarionProject) ? ((CommonClarionProject)pc.Project).ProgramFileName : null);
		if (!string.IsNullOrEmpty(text))
		{
			ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(text);
			if (parseInformationIfExist != null && parseInformationIfExist.MostRecentCompilationUnit is ClaCompilationUnit)
			{
				ClaCompilationUnit claCompilationUnit = (ClaCompilationUnit)(object)parseInformationIfExist.MostRecentCompilationUnit;
				globalClass = (IClass)(object)claCompilationUnit.GlobalClass;
				if (globalClass != null)
				{
					list.AddRange(SearchMethod(globalClass.DefaultReturnType, memberName));
				}
			}
		}
		return list;
	}

	public override List<IMethod> SearchMethod(IReturnType type, string memberName)
	{
		List<IMethod> list = new List<IMethod>();
		if (type == null)
		{
			return list;
		}
		bool flag = false;
		if (callingClass != null)
		{
			flag = callingClass.IsTypeInInheritanceTree(type.GetUnderlyingClass());
		}
		foreach (IMethod method in type.GetMethods())
		{
			if (IsSameName(((IMember)method).Name, memberName) && ((IDecoration)method).IsAccessible(callingClass, flag))
			{
				list.Add(method);
			}
		}
		return list;
	}

	public override IReturnType SearchMember(IReturnType type, string memberName)
	{
		if (type == null)
		{
			return null;
		}
		IMember member = GetMember(type, memberName);
		if (member == null)
		{
			return null;
		}
		return member.ReturnType;
	}

	public IMember GetMember(IReturnType type, string memberName)
	{
		if (type == null)
		{
			return null;
		}
		foreach (IProperty property in type.GetProperties())
		{
			if (IsSameName(((IMember)property).Name, memberName))
			{
				return (IMember)(object)property;
			}
		}
		foreach (IField field in type.GetFields())
		{
			if (IsSameName(((IMember)field).Name, memberName))
			{
				return (IMember)(object)field;
			}
		}
		foreach (IEvent @event in type.GetEvents())
		{
			if (IsSameName(((IMember)@event).Name, memberName))
			{
				return (IMember)(object)@event;
			}
		}
		if (type.GetUnderlyingClass() is ClaClass)
		{
			ClaClass claClass = (ClaClass)(object)type.GetUnderlyingClass();
			if (claClass.ClarionType == ClarionType.FILE)
			{
				foreach (IClass innerClass in claClass.InnerClasses)
				{
					if (!(innerClass is ClaClass) || ((ClaClass)(object)innerClass).ClarionType != ClarionType.RECORD)
					{
						continue;
					}
					foreach (IField field2 in innerClass.Fields)
					{
						if (IsSameName(((IMember)field2).Name, memberName))
						{
							return (IMember)(object)field2;
						}
					}
					break;
				}
			}
		}
		return null;
	}

	public override IReturnType DynamicLookup(IdentifierExpression identifier)
	{
		ResolveResult val = ResolveIdentifierInternal(identifier, onlyGlobalAndTypes: false, isInLike: false);
		if (val is NamespaceResolveResult)
		{
			return (IReturnType)(object)new ClaTypeVisitor.NamespaceReturnType(((NamespaceResolveResult)((val is NamespaceResolveResult) ? val : null)).Name);
		}
		if (val == null && identifier.Identifier.Contains(":"))
		{
			val = ResolveIdentifierWithPre(identifier.Identifier);
		}
		if (val == null)
		{
			return null;
		}
		return val.ResolvedType;
	}

	private IParameter SearchMethodParameter(IMember member, string parameter)
	{
		IMethodOrProperty val = (IMethodOrProperty)(object)((member is IMethodOrProperty) ? member : null);
		if (val == null)
		{
			return null;
		}
		foreach (IParameter parameter2 in val.Parameters)
		{
			if (parameter2 is ClaParameter)
			{
				if (IsSameName(((ClaParameter)(object)parameter2).DefName, parameter))
				{
					return parameter2;
				}
				if (IsSameName(((ClaParameter)(object)parameter2).DeclName, parameter))
				{
					return parameter2;
				}
			}
			else if (IsSameName(parameter2.Name, parameter))
			{
				return parameter2;
			}
		}
		return null;
	}

	private IClass GetPrimitiveClass(string systemType, ClarionType newName)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (systemType == null)
		{
			return null;
		}
		IClass val = pc.GetClass(systemType);
		if (val != null)
		{
			DefaultClass val2 = new DefaultClass(val.CompilationUnit, newName.ToString());
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

	public override ArrayList CtrlSpace(int caretLine, int caretColumn, string fileName, string fileContent, ExpressionContext context)
	{
		Initialize(fileName, fileContent, caretLine, caretColumn, null);
		ArrayList result = new ArrayList();
		ClaCtrlSpaceResolveHelper.AddContentsFromCalling(result, callingClass, callingMember);
		ClaCtrlSpaceResolveHelper.AddEquates(result, cu);
		return result;
	}
}
