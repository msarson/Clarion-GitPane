using System;
using System.Collections.Generic;
using System.Drawing;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Refactoring;

public static class RefactoringService
{
	public static List<IClass> FindDerivedClasses(IClass baseClass, IEnumerable<IProjectContent> projectContents, bool directDerivationOnly)
	{
		baseClass = baseClass.GetCompoundClass();
		string name = baseClass.Name;
		string fullyQualifiedName = baseClass.FullyQualifiedName;
		List<IClass> list = new List<IClass>();
		foreach (IProjectContent projectContent in projectContents)
		{
			if (projectContent == baseClass.ProjectContent || projectContent.ReferencedContents.Contains(baseClass.ProjectContent))
			{
				AddDerivedClasses(projectContent, baseClass, name, fullyQualifiedName, projectContent.Classes, list);
			}
		}
		if (!directDerivationOnly)
		{
			List<IClass> list2 = new List<IClass>();
			foreach (IClass item in list)
			{
				list2.AddRange(FindDerivedClasses(item, projectContents, directDerivationOnly));
			}
			foreach (IClass item2 in list2)
			{
				if (!list.Contains(item2))
				{
					list.Add(item2);
				}
			}
		}
		return list;
	}

	private static void AddDerivedClasses(IProjectContent pc, IClass baseClass, string baseClassName, string baseClassFullName, IEnumerable<IClass> classList, IList<IClass> resultList)
	{
		foreach (IClass @class in classList)
		{
			AddDerivedClasses(pc, baseClass, baseClassName, baseClassFullName, @class.InnerClasses, resultList);
			int count = @class.BaseTypes.Count;
			for (int i = 0; i < count; i++)
			{
				string name = @class.BaseTypes[i].Name;
				if (pc.Language.NameComparer.Equals(name, baseClassName) || pc.Language.NameComparer.Equals(name, baseClassFullName))
				{
					IReturnType baseType = @class.GetBaseType(i);
					if (baseType.FullyQualifiedName == baseClass.FullyQualifiedName)
					{
						resultList.Add(@class);
					}
				}
			}
		}
	}

	public static List<Reference> FindReferences(IMember member, IProgressNotificationTaskInstance progressMonitor)
	{
		return RunFindReferences(member.DeclaringType, member, isLocal: false, progressMonitor);
	}

	public static List<Reference> FindReferences(IClass @class, IProgressNotificationTaskInstance progressMonitor)
	{
		if (@class == null)
		{
			throw new ArgumentNullException("class");
		}
		return RunFindReferences(@class, null, isLocal: false, progressMonitor);
	}

	public static List<Reference> FindReferences(ResolveResult entity, IProgressNotificationTaskInstance progressMonitor)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		if (entity is LocalResolveResult)
		{
			return RunFindReferences(entity.CallingClass, (entity as LocalResolveResult).Field, isLocal: true, progressMonitor);
		}
		if (entity is TypeResolveResult)
		{
			return FindReferences((entity as TypeResolveResult).ResolvedClass, progressMonitor);
		}
		if (entity is MemberResolveResult)
		{
			return FindReferences((entity as MemberResolveResult).ResolvedMember, progressMonitor);
		}
		if (entity is MethodResolveResult)
		{
			IMethod methodIfSingleOverload = (entity as MethodResolveResult).GetMethodIfSingleOverload();
			if (methodIfSingleOverload != null)
			{
				return FindReferences(methodIfSingleOverload, progressMonitor);
			}
		}
		else if (entity is MixedResolveResult)
		{
			return FindReferences((entity as MixedResolveResult).PrimaryResult, progressMonitor);
		}
		return null;
	}

	private static List<Reference> RunFindReferences(IClass ownerClass, IMember member, bool isLocal, IProgressNotificationTaskInstance progressMonitor)
	{
		if (ParserService.LoadSolutionProjectsThreadRunning)
		{
			MessageService.ShowMessage("${res:SharpDevelop.Refactoring.LoadSolutionProjectsThreadRunning}");
			return null;
		}
		List<ProjectItem> list;
		if (isLocal)
		{
			list = new List<ProjectItem>();
			list.Add(FindItem(ownerClass.CompilationUnit.FileName));
		}
		else
		{
			ownerClass = ownerClass.GetCompoundClass();
			list = GetPossibleFiles(ownerClass, member);
		}
		ParseableFileContentEnumerator parseableFileContentEnumerator = new ParseableFileContentEnumerator(list.ToArray());
		List<Reference> list2 = new List<Reference>();
		try
		{
			progressMonitor?.BeginTask("${res:SharpDevelop.Refactoring.FindingReferences}", list.Count, allowCancel: true);
			while (parseableFileContentEnumerator.MoveNext())
			{
				if (progressMonitor != null)
				{
					progressMonitor.WorkDone = parseableFileContentEnumerator.Index;
					if (progressMonitor.IsCancelled)
					{
						return null;
					}
				}
				AddReferences(list2, ownerClass, member, isLocal, parseableFileContentEnumerator.CurrentFileName, parseableFileContentEnumerator.CurrentFileContent);
			}
			return list2;
		}
		finally
		{
			progressMonitor?.Done();
			parseableFileContentEnumerator.Dispose();
		}
	}

	private static void AddReferences(List<Reference> list, IClass parentClass, IMember member, bool isLocal, string fileName, string fileContent)
	{
		string text = fileContent.ToLowerInvariant();
		bool flag = false;
		string text2;
		if (member == null)
		{
			text2 = parentClass.Name.ToLowerInvariant();
		}
		else if (member is IMethod && ((IMethod)member).IsConstructor)
		{
			text2 = parentClass.Name.ToLowerInvariant();
		}
		else if (member is IProperty && ((IProperty)member).IsIndexer)
		{
			flag = true;
			text2 = GetIndexerExpressionStartToken(fileName);
		}
		else
		{
			text2 = member.Name.ToLowerInvariant();
		}
		if (text2.Length == 0)
		{
			return;
		}
		int num = -1;
		IExpressionFinder expressionFinder = null;
		while ((num = text.IndexOf(text2, num + 1)) >= 0)
		{
			int num2;
			if (!flag)
			{
				if ((num > 0 && (char.IsLetterOrDigit(fileContent, num - 1) || fileContent[num - 1] == '_')) || (num < fileContent.Length - text2.Length - 1 && (char.IsLetterOrDigit(fileContent, num + text2.Length) || fileContent[num + text2.Length] == '_')))
				{
					continue;
				}
				num2 = num;
			}
			else
			{
				num2 = num - 1;
			}
			if (expressionFinder == null)
			{
				expressionFinder = ParserService.GetExpressionFinder(fileName);
			}
			ExpressionResult expr = expressionFinder.FindFullExpression(fileContent, num2);
			if (expr.Expression == null)
			{
				continue;
			}
			Point position = GetPosition(fileContent, num2);
			while (true)
			{
				ResolveResult resolveResult = ParserService.Resolve(expr, position.Y, position.X, fileName, fileContent);
				MemberResolveResult memberResolveResult = resolveResult as MemberResolveResult;
				if (isLocal)
				{
					if (IsReferenceToLocalVariable(resolveResult, member))
					{
						list.Add(new Reference(fileName, num, text2.Length, expr.Expression, resolveResult));
						break;
					}
					if (!FixIndexerExpression(expressionFinder, ref expr, memberResolveResult))
					{
						break;
					}
					continue;
				}
				if (member != null)
				{
					if (IsReferenceToMember(member, resolveResult))
					{
						list.Add(new Reference(fileName, num, text2.Length, expr.Expression, resolveResult));
						break;
					}
					if (!FixIndexerExpression(expressionFinder, ref expr, memberResolveResult))
					{
						break;
					}
					continue;
				}
				if (memberResolveResult != null)
				{
					if (memberResolveResult.ResolvedMember is IMethod && ((IMethod)memberResolveResult.ResolvedMember).IsConstructor && memberResolveResult.ResolvedMember.DeclaringType.FullyQualifiedName == parentClass.FullyQualifiedName)
					{
						list.Add(new Reference(fileName, num, text2.Length, expr.Expression, resolveResult));
					}
					break;
				}
				if (resolveResult is MixedResolveResult)
				{
					resolveResult = ((MixedResolveResult)resolveResult).TypeResult;
				}
				if (resolveResult is TypeResolveResult && resolveResult.ResolvedType.FullyQualifiedName == parentClass.FullyQualifiedName)
				{
					list.Add(new Reference(fileName, num, text2.Length, expr.Expression, resolveResult));
				}
				break;
			}
		}
	}

	public static bool FixIndexerExpression(IExpressionFinder expressionFinder, ref ExpressionResult expr, MemberResolveResult mrr)
	{
		if (mrr != null && mrr.ResolvedMember is IProperty && ((IProperty)mrr.ResolvedMember).IsIndexer)
		{
			string text = expressionFinder.RemoveLastPart(expr.Expression);
			if (text.Length >= expr.Expression.Length)
			{
				throw new ApplicationException("new expression must be shorter than old expression");
			}
			expr.Expression = text;
			return true;
		}
		return false;
	}

	private static string GetIndexerExpressionStartToken(string fileName)
	{
		if (fileName != null)
		{
			ParseInformation parseInformation = ParserService.GetParseInformation(fileName);
			if (parseInformation != null && parseInformation.MostRecentCompilationUnit != null && parseInformation.MostRecentCompilationUnit.ProjectContent != null && parseInformation.MostRecentCompilationUnit.ProjectContent.Language != null)
			{
				return parseInformation.MostRecentCompilationUnit.ProjectContent.Language.IndexerExpressionStartToken;
			}
		}
		LoggingService.Warn("RefactoringService: unable to determine the correct indexer expression start token for file '" + fileName + "'");
		return LanguageProperties.CSharp.IndexerExpressionStartToken;
	}

	private static Point GetPosition(string fileContent, int pos)
	{
		int num = 1;
		int num2 = 0;
		for (int i = 0; i < pos; i++)
		{
			if (fileContent[i] == '\n')
			{
				num++;
				num2 = 0;
			}
			else
			{
				num2++;
			}
		}
		return new Point(num2, num);
	}

	private static List<string> GetFileNames(IClass c)
	{
		List<string> list = new List<string>();
		if (c is CompoundClass compoundClass)
		{
			foreach (IClass part in compoundClass.GetParts())
			{
				string fileName = part.CompilationUnit.FileName;
				if (fileName != null)
				{
					list.Add(fileName);
				}
			}
		}
		else
		{
			string fileName2 = c.CompilationUnit.FileName;
			if (fileName2 != null)
			{
				list.Add(fileName2);
			}
		}
		return list;
	}

	private static List<ProjectItem> GetPossibleFiles(IClass ownerClass, IDecoration member)
	{
		List<ProjectItem> list = new List<ProjectItem>();
		if (ProjectService.OpenSolution == null)
		{
			foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				string text = item.FileName ?? item.UntitledName;
				if (ParserService.GetParser(text) != null)
				{
					FileProjectItem fileProjectItem = new FileProjectItem(null, ItemType.Compile);
					fileProjectItem.Include = text;
					list.Add(fileProjectItem);
				}
			}
			return list;
		}
		if (member == null)
		{
			while (ownerClass.DeclaringType != null)
			{
				member = ownerClass;
				ownerClass = ownerClass.DeclaringType;
			}
			if (member == null)
			{
				GetPossibleFilesInternal(list, ownerClass.ProjectContent, ownerClass.IsInternal);
				return list;
			}
		}
		if (member.IsPrivate)
		{
			List<string> fileNames = GetFileNames(ownerClass);
			{
				foreach (string item2 in fileNames)
				{
					ProjectItem projectItem = FindItem(item2);
					if (projectItem != null)
					{
						list.Add(projectItem);
					}
				}
				return list;
			}
		}
		_ = member.IsProtected;
		GetPossibleFilesInternal(list, ownerClass.ProjectContent, ownerClass.IsInternal || (member.IsInternal && !member.IsProtected));
		return list;
	}

	private static ProjectItem FindItem(string fileName)
	{
		if (ProjectService.OpenSolution != null)
		{
			foreach (IProject project in ProjectService.OpenSolution.Projects)
			{
				foreach (ProjectItem item in project.Items)
				{
					if (FileUtility.IsEqualFileName(fileName, item.FileName))
					{
						return item;
					}
				}
			}
		}
		FileProjectItem fileProjectItem = new FileProjectItem(null, ItemType.Compile);
		fileProjectItem.Include = fileName;
		return fileProjectItem;
	}

	private static void GetPossibleFilesInternal(List<ProjectItem> resultList, IProjectContent ownerProjectContent, bool internalOnly)
	{
		if (ProjectService.OpenSolution == null)
		{
			return;
		}
		foreach (IProject project in ProjectService.OpenSolution.Projects)
		{
			IProjectContent projectContent = ParserService.GetProjectContent(project);
			if (projectContent == null || (projectContent != ownerProjectContent && (internalOnly || !projectContent.ReferencedContents.Contains(ownerProjectContent))))
			{
				continue;
			}
			foreach (ProjectItem item in project.Items)
			{
				if (item.ItemType == ItemType.Compile)
				{
					resultList.Add(item);
				}
			}
		}
	}

	public static bool IsReferenceToLocalVariable(ResolveResult rr, IMember variable)
	{
		if (!(rr is LocalResolveResult localResolveResult))
		{
			return false;
		}
		if (localResolveResult.Field.Region.BeginLine == variable.Region.BeginLine)
		{
			return localResolveResult.Field.Region.BeginColumn == variable.Region.BeginColumn;
		}
		return false;
	}

	public static bool IsReferenceToMember(IMember member, ResolveResult rr)
	{
		if (rr is MemberResolveResult memberResolveResult)
		{
			return IsSimilarMember(memberResolveResult.ResolvedMember, member);
		}
		if (rr is MethodResolveResult)
		{
			return IsSimilarMember((rr as MethodResolveResult).GetMethodIfSingleOverload(), member);
		}
		return false;
	}

	public static bool IsSimilarMember(IMember member1, IMember member2)
	{
		do
		{
			if (IsSimilarMemberInternal(member1, member2))
			{
				return true;
			}
		}
		while ((member1 = FindBaseMember(member1)) != null);
		return false;
	}

	private static bool IsSimilarMemberInternal(IMember member1, IMember member2)
	{
		if (member1 == member2)
		{
			return true;
		}
		if (member1 == null || member2 == null)
		{
			return false;
		}
		if (member1.FullyQualifiedName != member2.FullyQualifiedName)
		{
			return false;
		}
		if (member1.IsStatic != member2.IsStatic)
		{
			return false;
		}
		if (member1 is IMethod)
		{
			if (!(member2 is IMethod))
			{
				return false;
			}
			if (DiffUtility.Compare(((IMethod)member1).Parameters, ((IMethod)member2).Parameters) != 0)
			{
				return false;
			}
		}
		if (member1 is IProperty)
		{
			if (!(member2 is IProperty))
			{
				return false;
			}
			if (DiffUtility.Compare(((IProperty)member1).Parameters, ((IProperty)member2).Parameters) != 0)
			{
				return false;
			}
		}
		return true;
	}

	public static IMember FindSimilarMember(IClass type, IMember member)
	{
		if (member is IMethod)
		{
			IMethod method = (IMethod)member;
			foreach (IMethod method2 in type.Methods)
			{
				if (string.Equals(method.Name, method2.Name, StringComparison.InvariantCultureIgnoreCase) && method2.IsStatic == method.IsStatic && DiffUtility.Compare(method.Parameters, method2.Parameters) == 0)
				{
					return method2;
				}
			}
		}
		else if (member is IProperty)
		{
			IProperty property = (IProperty)member;
			foreach (IProperty property2 in type.Properties)
			{
				if (string.Equals(property.Name, property2.Name, StringComparison.InvariantCultureIgnoreCase) && property2.IsStatic == property.IsStatic && DiffUtility.Compare(property.Parameters, property2.Parameters) == 0)
				{
					return property2;
				}
			}
		}
		return null;
	}

	public static IMember FindBaseMember(IMember member)
	{
		if (member == null)
		{
			return null;
		}
		IClass declaringType = member.DeclaringType;
		IClass baseClass = declaringType.BaseClass;
		if (baseClass == null)
		{
			return null;
		}
		foreach (IClass item in baseClass.ClassInheritanceTree)
		{
			IMember member2 = FindSimilarMember(item, member);
			if (member2 != null)
			{
				return member2;
			}
		}
		return null;
	}
}
