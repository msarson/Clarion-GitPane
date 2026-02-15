using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Refactoring;

public static class NamespaceRefactoringService
{
	internal static bool IsSystemNamespace(string ns)
	{
		if (!ns.StartsWith("System."))
		{
			return ns == "System";
		}
		return true;
	}

	private static int CompareUsings(IUsing a, IUsing b)
	{
		if (a.HasAliases != b.HasAliases)
		{
			if (!a.HasAliases)
			{
				return -1;
			}
			return 1;
		}
		if (a.Usings.Count != 0 && b.Usings.Count != 0)
		{
			string ns = a.Usings[0];
			string ns2 = b.Usings[0];
			if (IsSystemNamespace(ns) && !IsSystemNamespace(ns2))
			{
				return -1;
			}
			if (!IsSystemNamespace(ns) && IsSystemNamespace(ns2))
			{
				return 1;
			}
			return a.Usings[0].CompareTo(b.Usings[0]);
		}
		if (a.Aliases.Count != 0 && b.Aliases.Count != 0)
		{
			return a.Aliases.Keys[0].CompareTo(b.Aliases.Keys[0]);
		}
		return 0;
	}

	public static void ManageUsings(string fileName, IDocument document, bool sort, bool removedUnused)
	{
		ParseInformation parseInformation = ParserService.ParseFile(fileName, document.TextContent);
		if (parseInformation == null)
		{
			return;
		}
		ICompilationUnit mostRecentCompilationUnit = parseInformation.MostRecentCompilationUnit;
		List<IUsing> list = new List<IUsing>(mostRecentCompilationUnit.Usings);
		if (sort)
		{
			list.Sort(CompareUsings);
		}
		if (removedUnused)
		{
			IList<IUsing> list2 = mostRecentCompilationUnit.ProjectContent.Language.RefactoringProvider.FindUnusedUsingDeclarations(fileName, document.TextContent, mostRecentCompilationUnit);
			if (list2 != null && list2.Count > 0)
			{
				foreach (IUsing item in list2)
				{
					string text = null;
					for (int i = 0; i < item.Usings.Count; i++)
					{
						text = item.Usings[i];
						if (text == "System")
						{
							break;
						}
					}
					if (text != "System")
					{
						list.Remove(item);
					}
				}
			}
		}
		if (sort && list.Count > 1 && list[0].Usings.Count > 0)
		{
			bool flag = IsSystemNamespace(list[0].Usings[0]);
			int num = 1;
			int num2 = 1;
			while (flag && num2 < list.Count)
			{
				flag = list[num2].Usings.Count > 0 && IsSystemNamespace(list[num2].Usings[0]);
				if (flag)
				{
					num++;
				}
				else if (num > 2)
				{
					list.Insert(num2, null);
				}
				num2++;
			}
		}
		mostRecentCompilationUnit.ProjectContent.Language.CodeGenerator.ReplaceUsings(new TextEditorDocument(document), mostRecentCompilationUnit.Usings, list);
	}
}
