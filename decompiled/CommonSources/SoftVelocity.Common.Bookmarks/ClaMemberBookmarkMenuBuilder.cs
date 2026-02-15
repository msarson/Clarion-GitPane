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
using ICSharpCode.TextEditor.Document;
using SearchAndReplace;
using SoftVelocity.Common.ClarionEditor;
using SoftVelocity.Common.ClassBrowser;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.Bookmarks;

public class ClaMemberBookmarkMenuBuilder : ISubmenuBuilder
{
	public virtual ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected O, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Expected O, but got Unknown
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Expected O, but got Unknown
		IMember member;
		if (owner is ClaMemberNode claMemberNode)
		{
			member = claMemberNode.Member;
		}
		else
		{
			ClaMemberBookmark claMemberBookmark = (ClaMemberBookmark)owner;
			member = claMemberBookmark.Member;
		}
		IMethod val = (IMethod)(object)((member is IMethod) ? member : null);
		List<ToolStripItem> list = new List<ToolStripItem>();
		if (!(member is ClaAbstractMember))
		{
			return list.ToArray();
		}
		ClaAbstractMember claAbstractMember = (ClaAbstractMember)(object)member;
		bool flag = false;
		if (val is ClaMethod && ((ClaMethod)(object)val).IsAccessor)
		{
			flag = true;
		}
		MenuCommand val2;
		if (!claAbstractMember.ClaRegion.IsEmpty)
		{
			val2 = new MenuCommand("Goto Declaration", (EventHandler)GotoDeclaration);
			((ToolStripItem)(object)val2).Tag = claAbstractMember;
			list.Add((ToolStripItem)(object)val2);
		}
		if (val is ClaMethod)
		{
			if (!((ClaMethod)(object)val).ClaBodyRegion.IsEmpty)
			{
				val2 = new MenuCommand("Goto Definition", (EventHandler)GotoDefinition);
				((ToolStripItem)(object)val2).Tag = claAbstractMember;
				list.Add((ToolStripItem)(object)val2);
			}
			else if (SourceFileImplementationExist(claAbstractMember))
			{
				val2 = new MenuCommand("Goto Implementation", (EventHandler)GotoSourceFileImplementation);
				((ToolStripItem)(object)val2).Tag = claAbstractMember;
				list.Add((ToolStripItem)(object)val2);
			}
		}
		if (flag)
		{
			return list.ToArray();
		}
		if ((val == null || !val.IsConstructor) && !FindReferencesAndRenameHelper.IsReadOnly(((IDecoration)member).DeclaringType) && (!(member is IProperty) || !((IProperty)member).IsIndexer) && !claAbstractMember.ClaRegion.IsEmpty)
		{
			val2 = new MenuCommand("${res:SharpDevelop.Refactoring.RenameCommand}", (EventHandler)Rename);
			((ToolStripItem)(object)val2).Tag = member;
			list.Add((ToolStripItem)(object)val2);
		}
		if (((IDecoration)member).IsOverride)
		{
			val2 = new MenuCommand("${res:SharpDevelop.Refactoring.GoToBaseClassCommand}", (EventHandler)GoToBase);
			((ToolStripItem)(object)val2).Tag = member;
			list.Add((ToolStripItem)(object)val2);
		}
		if (((IDecoration)member).IsVirtual || ((IDecoration)member).IsAbstract || (((IDecoration)member).IsOverride && !((IDecoration)((IDecoration)member).DeclaringType).IsSealed))
		{
			val2 = new MenuCommand("${res:SharpDevelop.Refactoring.FindOverridesCommand}", (EventHandler)FindOverrides);
			((ToolStripItem)(object)val2).Tag = member;
			list.Add((ToolStripItem)(object)val2);
		}
		val2 = new MenuCommand("${res:SharpDevelop.Refactoring.FindReferencesCommand}", (EventHandler)FindReferences);
		((ToolStripItem)(object)val2).Tag = member;
		list.Add((ToolStripItem)(object)val2);
		return list.ToArray();
	}

	private static void GotoDefinition(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		MenuCommand val = (MenuCommand)sender;
		ClaMethod claMethod = (ClaMethod)((ToolStripItem)(object)val).Tag;
		string fileName = claMethod.ClaBodyRegion.FileName;
		DomRegion bodyRegion = claMethod.BodyRegion;
		FileService.JumpToFilePosition(fileName, ((DomRegion)(ref bodyRegion)).BeginLine - 1, 0);
	}

	private static void GotoDeclaration(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		MenuCommand val = (MenuCommand)sender;
		ClaAbstractMember claAbstractMember = (ClaAbstractMember)((ToolStripItem)(object)val).Tag;
		string fileName = claAbstractMember.ClaRegion.FileName;
		DomRegion region = claAbstractMember.Region;
		FileService.JumpToFilePosition(fileName, ((DomRegion)(ref region)).BeginLine - 1, 0);
	}

	internal static string GetClassImplementationModuleName(ClaAbstractMember m)
	{
		if (m.DeclaringType is ClaClass)
		{
			ClaClass claClass = m.DeclaringType as ClaClass;
			if (!string.IsNullOrEmpty(claClass.DeclarationModule))
			{
				try
				{
					RedirectionFile val = CommonClarionProject.CurrentRedirectionFile(null);
					return val.OpenName(claClass.DeclarationModule, RedirectionFile.CurrentDirectory);
				}
				catch
				{
				}
			}
		}
		return string.Empty;
	}

	internal static string GetSourceFileImplementationName(ClaAbstractMember m)
	{
		string fileName = m.ClaRegion.FileName;
		if (!string.IsNullOrEmpty(fileName))
		{
			if (Path.GetExtension(fileName).Equals(".INC", StringComparison.OrdinalIgnoreCase))
			{
				fileName = Path.ChangeExtension(fileName, ".CLW");
				if (FileService.CheckFileName(fileName))
				{
					return fileName;
				}
			}
			else if (Path.GetExtension(fileName).Equals(".CLW", StringComparison.OrdinalIgnoreCase))
			{
				return fileName;
			}
		}
		return string.Empty;
	}

	private static bool SourceFileImplementationExist(ClaAbstractMember m)
	{
		if (m.DeclaringType is ClaClass)
		{
			ClaClass claClass = m.DeclaringType as ClaClass;
			if (claClass.ClarionType == ClarionType.INTERFACE || (claClass.DeclarationModule != null && claClass.DeclarationModule.Equals("ABSTRACT", StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}
		}
		string classImplementationModuleName = GetClassImplementationModuleName(m);
		if (!string.IsNullOrEmpty(classImplementationModuleName))
		{
			return true;
		}
		classImplementationModuleName = GetSourceFileImplementationName(m);
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
		ClaAbstractMember claAbstractMember = (ClaAbstractMember)((ToolStripItem)(object)val).Tag;
		string text = GetClassImplementationModuleName(claAbstractMember);
		if (string.IsNullOrEmpty(text))
		{
			text = GetSourceFileImplementationName(claAbstractMember);
		}
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		IWorkbenchWindow val2 = null;
		val2 = ((!FileService.IsOpen(text)) ? FileService.OpenFile(text) : FileService.GetOpenFile(text));
		if (val2 == null)
		{
			return;
		}
		val2.SelectWindow();
		Application.DoEvents();
		WorkbenchSingleton.Workbench.ShowView(val2.ViewContent);
		string serachText = claAbstractMember.DeclaringType.Name + "." + claAbstractMember.Name;
		if (!(val2.ActiveViewContent.Control is ClarionCommonTextAreaControl clarionCommonTextAreaControl))
		{
			return;
		}
		((Control)(object)clarionCommonTextAreaControl).Select();
		Application.DoEvents();
		SearchReplaceUtilities.GetActiveTextEditor();
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName == text)
		{
			int line = 0;
			int column = 0;
			if (TryFindImplementation(serachText, out line, out column))
			{
				FileService.JumpToFilePosition(text, line, column);
			}
		}
	}

	private static bool TryFindImplementation(string serachText, out int line, out int column)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		line = 0;
		column = 0;
		SearchOptions.Preserve();
		SearchOptions.FindPattern = serachText;
		SearchOptions.MatchWholeWord = true;
		SearchOptions.MatchCase = false;
		SearchOptions.SearchStrategyType = (SearchStrategyType)0;
		SearchOptions.LookIn = "";
		SearchOptions.LookInFiletypes = "";
		SearchOptions.ReplacePattern = "";
		SearchOptions.IncludeSubdirectories = true;
		SearchOptions.SearchAndReplaceBinding = SearchOptions.CurrentDocumentBinding;
		ProgressNotificationTaskInstance val = new ProgressNotificationTaskInstance("Searching: " + serachText);
		try
		{
			Search val2 = new Search();
			val2.TextIteratorBuilder = (ITextIteratorBuilder)new ForwardTextIteratorBuilder();
			val2.SearchStrategy = SearchReplaceUtilities.CreateSearchStrategy(SearchOptions.SearchStrategyType);
			val2.DocumentIterator = SearchOptions.SearchAndReplaceBinding.GetIterator();
			val2.Reset();
			if (!val2.SearchStrategy.CompilePattern((IProgressNotificationTaskInstance)(object)val))
			{
				return false;
			}
			SearchResult val3 = val2.FindNext((IProgressNotificationTaskInstance)(object)val);
			while (val3 != null)
			{
				val3.CreateDocument();
				IDocument val4 = val3.CreateDocument();
				if (val4 == null)
				{
					return false;
				}
				TextLocation startPosition = val3.GetStartPosition(val4);
				line = ((TextLocation)(ref startPosition)).Line;
				column = ((TextLocation)(ref startPosition)).Column;
				if (column == 0)
				{
					break;
				}
			}
			if (val3 == null)
			{
				return false;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		SearchOptions.Restore();
		return true;
	}

	private static void GoToBase(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		MenuCommand val = (MenuCommand)sender;
		IMember val2 = (IMember)((ToolStripItem)(object)val).Tag;
		IMember val3 = RefactoringService.FindBaseMember(val2);
		if (val3 == null)
		{
			return;
		}
		string text = null;
		int num = -1;
		if (val3 is ClaMethod)
		{
			ClaMethod claMethod = (ClaMethod)(object)val3;
			if (claMethod.ClaBodyRegion.IsEmpty)
			{
				text = claMethod.ClaRegion.FileName;
				num = claMethod.ClaRegion.BeginLine - 1;
			}
			else
			{
				text = claMethod.ClaBodyRegion.FileName;
				num = claMethod.ClaBodyRegion.BeginLine - 1;
			}
		}
		else if (val3 is ClaAbstractMember)
		{
			text = ((ClaAbstractMember)(object)val3).ClaRegion.FileName;
			num = ((ClaAbstractMember)(object)val3).ClaRegion.BeginLine - 1;
		}
		else
		{
			ICompilationUnit compilationUnit = ((IDecoration)val3).DeclaringType.CompilationUnit;
			if (compilationUnit != null)
			{
				text = compilationUnit.FileName;
				DomRegion region = val3.Region;
				num = ((DomRegion)(ref region)).BeginLine - 1;
			}
		}
		if (text != null)
		{
			if (num >= 0)
			{
				FileService.JumpToFilePosition(text, num, 0);
			}
			else
			{
				FileService.OpenFile(text);
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
		FindReferencesAndRenameHelper.RenameMember((IMember)((ToolStripItem)(object)val).Tag);
	}

	private static void FindOverrides(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		MenuCommand val = (MenuCommand)sender;
		IMember val2 = (IMember)((ToolStripItem)(object)val).Tag;
		List<IClass> list = RefactoringService.FindDerivedClasses(((IDecoration)val2).DeclaringType, ParserService.AllProjectContents, false);
		List<SearchResult> list2 = new List<SearchResult>();
		foreach (IClass item in list)
		{
			if (item.CompilationUnit == null || item.CompilationUnit.FileName == null)
			{
				continue;
			}
			IMember val3 = RefactoringService.FindSimilarMember(item, val2);
			if (val3 == null)
			{
				continue;
			}
			DomRegion region = val3.Region;
			if (!((DomRegion)(ref region)).IsEmpty)
			{
				string fileName;
				int num;
				if (val3 is ClaMethod && !((ClaMethod)(object)val3).ClaBodyRegion.IsEmpty)
				{
					fileName = ((ClaMethod)(object)val3).ClaBodyRegion.FileName;
					num = ((ClaMethod)(object)val3).ClaBodyRegion.BeginLine - 1;
				}
				else if (val3 is ClaAbstractMember)
				{
					fileName = ((ClaAbstractMember)(object)val3).ClaRegion.FileName;
					num = ((ClaAbstractMember)(object)val3).ClaRegion.BeginLine - 1;
				}
				else
				{
					fileName = item.CompilationUnit.FileName;
					DomRegion region2 = val3.Region;
					num = ((DomRegion)(ref region2)).BeginLine - 1;
				}
				SearchResult val4 = (SearchResult)new SimpleSearchResult(val3.FullyQualifiedName, new TextLocation(0, num));
				val4.ProvidedDocumentInformation = FindReferencesAndRenameHelper.GetDocumentInformation(fileName);
				list2.Add(val4);
			}
		}
		SearchInFilesManager.ShowSearchResults("Overrides of " + val2.Name, list2);
	}

	private static void FindReferences(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		MenuCommand val = (MenuCommand)sender;
		IMember val2 = (IMember)((ToolStripItem)(object)val).Tag;
		if (val2 is ClaLocalVariableField)
		{
			LocalResolveResult val3 = new LocalResolveResult(((ClaLocalVariableField)(object)val2).DeclaringMethod, (IField)val2);
			List<Reference> list = RefactoringService.FindReferences((ResolveResult)(object)val3, (IProgressNotificationTaskInstance)null);
			FindReferencesAndRenameHelper.ShowAsSearchResults("References to " + ((IMember)val3.Field).Name, list);
		}
		else
		{
			string text = ((!(val2 is IProperty) || !((IProperty)val2).IsIndexer) ? val2.Name : ("indexer of " + ((IDecoration)val2).DeclaringType.Name));
			FindReferencesAndRenameHelper.ShowAsSearchResults("References to " + text, RefactoringService.FindReferences(val2, (IProgressNotificationTaskInstance)null));
		}
	}
}
