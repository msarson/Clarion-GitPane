using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Refactoring;
using ICSharpCode.TextEditor;
using SearchAndReplace;
using SoftVelocity.Common.ClassBrowser;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.Bookmarks;

public class ClaClassBookmarkMenuBuilder : ISubmenuBuilder
{
	public virtual ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Expected O, but got Unknown
		IClass val;
		if (owner is ClaClassNode claClassNode)
		{
			val = claClassNode.Class;
		}
		else
		{
			ClaClassBookmark claClassBookmark = (ClaClassBookmark)owner;
			val = claClassBookmark.Class;
		}
		List<ToolStripItem> list = new List<ToolStripItem>();
		if (!(val is ClaClass))
		{
			return list.ToArray();
		}
		ClaClass claClass = (ClaClass)(object)val;
		if (!claClass.ClaRegion.IsEmpty)
		{
			MenuCommand val2 = new MenuCommand("Goto Declaration", (EventHandler)GotoDeclaration);
			((ToolStripItem)(object)val2).Tag = claClass;
			list.Add((ToolStripItem)(object)val2);
			if (SourceFileImplementationExist(claClass))
			{
				val2 = new MenuCommand("Goto Implementation Source", (EventHandler)GotoSourceFileImplementation);
				((ToolStripItem)(object)val2).Tag = claClass;
				list.Add((ToolStripItem)(object)val2);
			}
		}
		if (!(claClass is ClaGlobalClass) && !FindReferencesAndRenameHelper.IsReadOnly((IClass)(object)claClass))
		{
			MenuCommand val2 = new MenuCommand("${res:SharpDevelop.Refactoring.RenameCommand}", (EventHandler)Rename);
			((ToolStripItem)(object)val2).Tag = claClass;
			list.Add((ToolStripItem)(object)val2);
		}
		if (claClass.BaseClass != null && claClass.BaseClass != ((ClaCompilationUnit)(object)claClass.CompilationUnit).FakeParentClass)
		{
			MenuCommand val2 = new MenuCommand("${res:SharpDevelop.Refactoring.GoToBaseCommand}", (EventHandler)GoToBase);
			((ToolStripItem)(object)val2).Tag = claClass;
			list.Add((ToolStripItem)(object)val2);
		}
		if (!claClass.IsSealed && !claClass.IsStatic)
		{
			MenuCommand val2 = new MenuCommand("${res:SharpDevelop.Refactoring.FindDerivedClassesCommand}", (EventHandler)FindDerivedClasses);
			((ToolStripItem)(object)val2).Tag = claClass;
			list.Add((ToolStripItem)(object)val2);
		}
		if (!(claClass is ClaGlobalClass))
		{
			MenuCommand val2 = new MenuCommand("${res:SharpDevelop.Refactoring.FindReferencesCommand}", (EventHandler)FindReferences);
			((ToolStripItem)(object)val2).Tag = claClass;
			list.Add((ToolStripItem)(object)val2);
		}
		return list.ToArray();
	}

	private static void GotoDeclaration(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		MenuCommand val = (MenuCommand)sender;
		ClaClass claClass = (ClaClass)((ToolStripItem)(object)val).Tag;
		string fileName = claClass.ClaRegion.FileName;
		DomRegion region = claClass.Region;
		FileService.JumpToFilePosition(fileName, ((DomRegion)(ref region)).BeginLine - 1, 0);
	}

	internal static string GetClassImplementationModuleName(ClaClass claCl)
	{
		if (!string.IsNullOrEmpty(claCl.DeclarationModule))
		{
			try
			{
				RedirectionFile val = CommonClarionProject.CurrentRedirectionFile(null);
				return val.OpenName(claCl.DeclarationModule, RedirectionFile.CurrentDirectory);
			}
			catch
			{
			}
		}
		return string.Empty;
	}

	internal static string GetSourceFileImplementationName(ClaClass c)
	{
		string fileName = c.ClaRegion.FileName;
		if (Path.GetExtension(fileName).Equals(".inc", StringComparison.OrdinalIgnoreCase))
		{
			fileName = Path.ChangeExtension(fileName, ".CLW");
			if (FileService.CheckFileName(fileName))
			{
				return fileName;
			}
		}
		return string.Empty;
	}

	private static bool SourceFileImplementationExist(ClaClass c)
	{
		if (c.ClarionType == ClarionType.INTERFACE || (c.DeclarationModule != null && c.DeclarationModule.Equals("ABSTRACT", StringComparison.OrdinalIgnoreCase)))
		{
			return false;
		}
		string classImplementationModuleName = GetClassImplementationModuleName(c);
		if (!string.IsNullOrEmpty(classImplementationModuleName))
		{
			return true;
		}
		classImplementationModuleName = GetSourceFileImplementationName(c);
		if (!string.IsNullOrEmpty(classImplementationModuleName))
		{
			return true;
		}
		return false;
	}

	private static void GotoSourceFileImplementation(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		MenuCommand val = (MenuCommand)sender;
		ClaClass claClass = (ClaClass)((ToolStripItem)(object)val).Tag;
		string text = GetClassImplementationModuleName(claClass);
		if (string.IsNullOrEmpty(text))
		{
			text = GetSourceFileImplementationName(claClass);
		}
		if (FileService.IsOpen(text))
		{
			FileService.GetOpenFile(text).SelectWindow();
		}
		else
		{
			FileService.OpenFile(text).SelectWindow();
		}
	}

	private static void GoToBase(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		MenuCommand val = (MenuCommand)sender;
		IClass val2 = (IClass)((ToolStripItem)(object)val).Tag;
		IClass baseClass = val2.BaseClass;
		if (baseClass != null)
		{
			string fileName = baseClass.CompilationUnit.FileName;
			if (baseClass is ClaClass && !((ClaClass)(object)baseClass).ClaRegion.IsEmpty)
			{
				fileName = ((ClaClass)(object)baseClass).ClaRegion.FileName;
			}
			if (fileName != null)
			{
				string text = fileName;
				DomRegion region = baseClass.Region;
				int num = ((DomRegion)(ref region)).BeginLine - 1;
				DomRegion region2 = baseClass.Region;
				FileService.JumpToFilePosition(text, num, ((DomRegion)(ref region2)).BeginColumn - 1);
			}
		}
	}

	private static void Rename(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		MenuCommand val = (MenuCommand)sender;
		FindReferencesAndRenameHelper.RenameClass((IClass)((ToolStripItem)(object)val).Tag);
	}

	private static void FindDerivedClasses(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		MenuCommand val = (MenuCommand)sender;
		IClass val2 = (IClass)((ToolStripItem)(object)val).Tag;
		List<IClass> list = RefactoringService.FindDerivedClasses(val2, ParserService.AllProjectContents, false);
		List<SearchResult> list2 = new List<SearchResult>();
		foreach (IClass item in list)
		{
			if (item.CompilationUnit != null && item.CompilationUnit.FileName != null)
			{
				string fullyQualifiedName = item.FullyQualifiedName;
				DomRegion region = item.Region;
				int num = ((DomRegion)(ref region)).BeginColumn - 1;
				DomRegion region2 = item.Region;
				SearchResult val3 = (SearchResult)new SimpleSearchResult(fullyQualifiedName, new TextLocation(num, ((DomRegion)(ref region2)).BeginLine - 1));
				val3.ProvidedDocumentInformation = FindReferencesAndRenameHelper.GetDocumentInformation(item.CompilationUnit.FileName);
				list2.Add(val3);
			}
		}
		SearchInFilesManager.ShowSearchResults("Classes deriving from " + val2.Name, list2);
	}

	private static void FindReferences(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		MenuCommand val = (MenuCommand)sender;
		IClass val2 = (IClass)((ToolStripItem)(object)val).Tag;
		FindReferencesAndRenameHelper.ShowAsSearchResults("References to " + val2.Name, RefactoringService.FindReferences(val2, (IProgressNotificationTaskInstance)null));
	}
}
