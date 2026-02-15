using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Refactoring;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.ClarionEditor;
using SoftVelocity.Common.ClassBrowser;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.Bookmarks;

public class ClaRefactoringMenuBuilder : ISubmenuBuilder
{
	protected CommonClarionEditor claEditor;

	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Expected O, but got Unknown
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Expected O, but got Unknown
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Expected O, but got Unknown
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Expected O, but got Unknown
		TextEditorControl val = (TextEditorControl)owner;
		if (((TextEditorControlBase)val).FileName == null)
		{
			return new ToolStripItem[0];
		}
		if (!(owner is ClarionCommonTextAreaControl))
		{
			return new ToolStripItem[0];
		}
		claEditor = ((ClarionCommonTextAreaControl)owner).ClaEditor;
		List<ToolStripItem> list = new List<ToolStripItem>();
		TextArea textArea = ((TextEditorControlBase)val).ActiveTextAreaControl.TextArea;
		IDocument document = textArea.Document;
		int line = textArea.Caret.Line;
		List<string> list2 = new List<string>();
		foreach (Bookmark mark in document.BookmarkManager.Marks)
		{
			if (mark == null || mark.LineNumber != line)
			{
				continue;
			}
			ClaMemberBookmark claMemberBookmark = mark as ClaMemberBookmark;
			ClaClassBookmark claClassBookmark = mark as ClaClassBookmark;
			bool flag = claMemberBookmark?.ShowMenu ?? claClassBookmark?.ShowMenu ?? false;
			IClass val2 = null;
			if (!flag)
			{
				continue;
			}
			if (claMemberBookmark != null)
			{
				ToolStripMenuItem toolStripMenuItem = MakeItem(list2, claMemberBookmark.Member);
				if (toolStripMenuItem != null)
				{
					list.Add(toolStripMenuItem);
				}
				val2 = ((IDecoration)claMemberBookmark.Member).DeclaringType;
			}
			else if (claClassBookmark != null)
			{
				val2 = claClassBookmark.Class;
			}
			if (val2 != null && !(val2 is ClaGlobalClass))
			{
				list2.Add(val2.DotNetName);
				int sortOrder = 0;
				int iconIndexForClass = ClaClassNode.GetIconIndexForClass(val2, ref sortOrder);
				ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem(val2.Name, ClassBrowserIconService.ImageList.Images[iconIndexForClass]);
				MenuService.AddItemsToMenu(toolStripMenuItem2.DropDown.Items, (object)(claClassBookmark ?? new ClaClassBookmark(textArea.Document, val2, showMenu: true)), claEditor.ClassBookmarkContextMenuPath);
				list.Add(toolStripMenuItem2);
			}
		}
		IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(((TextEditorControlBase)val).FileName);
		if (expressionFinder != null)
		{
			int index = list.Count;
			ExpressionResult expressionResult = FindFullExpressionAtCaret(textArea, expressionFinder);
			ResolveResult val3;
			ToolStripMenuItem toolStripMenuItem2;
			while (true)
			{
				val3 = ResolveExpressionAtCaret(textArea, expressionResult);
				toolStripMenuItem2 = null;
				if (val3 is MethodResolveResult)
				{
					toolStripMenuItem2 = MakeItem(list2, (IMember)(object)((MethodResolveResult)val3).GetMethodIfSingleOverload());
					break;
				}
				if (val3 is MemberResolveResult)
				{
					MemberResolveResult val4 = (MemberResolveResult)val3;
					toolStripMenuItem2 = MakeItem(list2, val4.ResolvedMember);
					if (!RefactoringService.FixIndexerExpression(expressionFinder, ref expressionResult, val4))
					{
						break;
					}
					if (toolStripMenuItem2 != null)
					{
						list.Insert(index, toolStripMenuItem2);
					}
					continue;
				}
				if (val3 is TypeResolveResult)
				{
					toolStripMenuItem2 = MakeItem(list2, ((TypeResolveResult)val3).ResolvedClass);
				}
				else if (val3 is LocalResolveResult)
				{
					LocalResolveResult val5 = (LocalResolveResult)val3;
					int num = line + 1;
					DomRegion region = ((IMember)((LocalResolveResult)val3).Field).Region;
					toolStripMenuItem2 = MakeItem(list2, val5, num == ((DomRegion)(ref region)).BeginLine);
					index = 0;
				}
				else if (val3 is UnknownIdentifierResolveResult)
				{
					toolStripMenuItem2 = MakeItemForResolveError((UnknownIdentifierResolveResult)(object)val3, expressionResult.Context, textArea);
					index = 0;
				}
				break;
			}
			if (toolStripMenuItem2 != null)
			{
				list.Insert(index, toolStripMenuItem2);
			}
			IMember val6 = null;
			if (val3 != null)
			{
				val6 = val3.CallingMember;
			}
			else
			{
				ParseInformation parseInformation = ParserService.GetParseInformation(((TextEditorControlBase)val).FileName);
				if (parseInformation != null)
				{
					ICompilationUnit mostRecentCompilationUnit = parseInformation.MostRecentCompilationUnit;
					if (mostRecentCompilationUnit != null)
					{
						if (mostRecentCompilationUnit is ClaCompilationUnit)
						{
							object obj = ((ClaCompilationUnit)(object)mostRecentCompilationUnit).FindNearestObject(line + 1, textArea.Caret.Column + 1);
							if (obj is IMember)
							{
								val6 = (IMember)obj;
							}
							if (val6 is ClaMethod && ((ClaMethod)(object)val6).IsAccessor)
							{
								val6 = (IMember)(object)((ClaMethod)(object)val6).DeclaringProperty;
							}
						}
						else
						{
							IClass innermostClass = mostRecentCompilationUnit.GetInnermostClass(line + 1, textArea.Caret.Column + 1);
							val6 = GetCallingMember(innermostClass, line + 1, textArea.Caret.Column + 1);
						}
					}
				}
			}
			if (val6 != null)
			{
				toolStripMenuItem2 = MakeItem(list2, val6);
				if (toolStripMenuItem2 != null)
				{
					toolStripMenuItem2.Text = StringParser.Parse("${res:SharpDevelop.Refactoring.CurrentMethod}: ") + val6.Name;
					list.Add(toolStripMenuItem2);
				}
			}
		}
		claEditor = null;
		if (list.Count == 0)
		{
			return new ToolStripItem[0];
		}
		list.Add((ToolStripItem)new MenuSeparator());
		return list.ToArray();
	}

	private static IMember GetCallingMember(IClass callingClass, int caretLine, int caretColumn)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (callingClass == null)
		{
			return null;
		}
		foreach (IMethod method in callingClass.Methods)
		{
			DomRegion bodyRegion = ((IMember)method).BodyRegion;
			if (((DomRegion)(ref bodyRegion)).IsInside(caretLine, caretColumn))
			{
				return (IMember)(object)method;
			}
		}
		foreach (IProperty property in callingClass.Properties)
		{
			DomRegion bodyRegion2 = ((IMember)property).BodyRegion;
			if (((DomRegion)(ref bodyRegion2)).IsInside(caretLine, caretColumn))
			{
				return (IMember)(object)property;
			}
		}
		return null;
	}

	private ToolStripMenuItem MakeItem(List<string> definitions, LocalResolveResult local, bool isDefinition)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (definitions.Contains(((IMember)local.Field).DotNetName))
		{
			return null;
		}
		definitions.Add(((IMember)local.Field).DotNetName);
		string fileName = ((local.Field is ClaAbstractMember) ? ((ClaAbstractMember)(object)local.Field).ClaRegion.FileName : null);
		ToolStripMenuItem toolStripMenuItem = MakeItemInternal(ClaMemberNode.GetText((IMember)(object)local.Field), local.IsParameter ? 17 : 16, ((ResolveResult)local).CallingClass.CompilationUnit, isDefinition ? DomRegion.Empty : ((IMember)local.Field).Region, fileName);
		string text = "/SharpDevelop/ViewContent/DefaultTextEditor/Refactoring/";
		text += (local.IsParameter ? "Parameter" : "LocalVariable");
		if (isDefinition)
		{
			text += "Definition";
		}
		MenuService.AddItemsToMenu(toolStripMenuItem.DropDown.Items, (object)local, text);
		return toolStripMenuItem;
	}

	private ToolStripMenuItem MakeItem(List<string> definitions, IMember member)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (member == null)
		{
			return null;
		}
		if (definitions.Contains(member.DotNetName))
		{
			return null;
		}
		definitions.Add(member.DotNetName);
		ClaMemberNode claMemberNode = ClaMemberNode.Create(null, member);
		((ExtTreeNode)claMemberNode).ContextmenuAddinTreePath = claEditor.MemberBookmarkContextMenuPath;
		string text = ((member is ClaAbstractMember) ? ((ClaAbstractMember)(object)member).ClaRegion.FileName : null);
		ClaAbstractMember claAbstractMember = member as ClaAbstractMember;
		if (claAbstractMember.DeclaringType is ClaClass)
		{
			ClaClass claClass = claAbstractMember.DeclaringType as ClaClass;
			if (!string.IsNullOrEmpty(claClass.DeclarationText))
			{
				_ = claClass.DeclarationText;
			}
		}
		ToolStripMenuItem toolStripMenuItem = MakeItem(member.FullyQualifiedName, (ExtTreeNode)(object)claMemberNode, ((ExtTreeNode)claMemberNode).ContextmenuAddinTreePath, ((IDecoration)member).DeclaringType.CompilationUnit, member.Region, text);
		ToolStripMenuItem toolStripMenuItem2 = MakeItem(null, ((IDecoration)member).DeclaringType);
		if (toolStripMenuItem2 != null && text != null)
		{
			if (((ExtTreeNode)claMemberNode).ContextmenuAddinTreePath != null)
			{
				toolStripMenuItem.DropDown.Items.Add(new ToolStripSeparator());
			}
			toolStripMenuItem2.Text = StringParser.Parse("${res:SharpDevelop.Refactoring.DeclaringType}: ") + toolStripMenuItem2.Text;
			toolStripMenuItem.DropDown.Items.Add(toolStripMenuItem2);
		}
		return toolStripMenuItem;
	}

	private ToolStripMenuItem MakeItem(List<string> definitions, IClass c)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (c == null)
		{
			return null;
		}
		if (definitions != null)
		{
			if (definitions.Contains(c.DotNetName))
			{
				return null;
			}
			definitions.Add(c.DotNetName);
		}
		string fileName = ((c is ClaAbstractMember) ? ((ClaAbstractMember)(object)c).ClaRegion.FileName : null);
		if (c.ProjectContent.Project == null)
		{
			ClassNode val = new ClassNode((IProject)c.ProjectContent.Project, c);
			return MakeItem(c.FullyQualifiedName, (ExtTreeNode)(object)val, ((ExtTreeNode)val).ContextmenuAddinTreePath, c.CompilationUnit, c.Region, fileName);
		}
		return MakeItem(c.FullyQualifiedName, (ExtTreeNode)(object)new ClaClassNode((IProject)c.ProjectContent.Project, c), claEditor.ClassBookmarkContextMenuPath, c.CompilationUnit, c.Region, fileName);
	}

	protected static ToolStripMenuItem MakeItemInternal(string title, int imageIndex, ICompilationUnit cu, DomRegion region, string fileName)
	{
		return new ToolStripMenuItem(title, ClassBrowserIconService.ImageList.Images[imageIndex]);
	}

	private static ToolStripMenuItem MakeItem(string title, ExtTreeNode classBrowserTreeNode, string contextMenuPath, ICompilationUnit cu, DomRegion region, string fileName)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		ToolStripMenuItem toolStripMenuItem = MakeItemInternal(((TreeNode)(object)classBrowserTreeNode).Text, ((TreeNode)(object)classBrowserTreeNode).ImageIndex, cu, region, fileName);
		MenuService.AddItemsToMenu(toolStripMenuItem.DropDown.Items, (object)classBrowserTreeNode, contextMenuPath);
		return toolStripMenuItem;
	}

	private ToolStripMenuItem MakeItemForResolveError(UnknownIdentifierResolveResult rr, ExpressionContext context, TextArea textArea)
	{
		return MakeItemForUnknownClass(((ResolveResult)rr).CallingClass, rr.Identifier, rr.TypeParametersCount, textArea);
	}

	protected virtual ToolStripMenuItem MakeItemForUnknownClass(IClass callingClass, string unknownClassName, int typeParametersCount, TextArea textArea)
	{
		return null;
	}

	private static ExpressionResult FindFullExpressionAtCaret(TextArea textArea, IExpressionFinder expressionFinder)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (expressionFinder != null)
		{
			return expressionFinder.FindFullExpression(textArea.Document.TextContent, textArea.Caret.Offset);
		}
		return new ExpressionResult((string)null);
	}

	private static ResolveResult ResolveExpressionAtCaret(TextArea textArea, ExpressionResult expressionResult)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (expressionResult.Expression != null)
		{
			return ParserService.Resolve(expressionResult, textArea.Caret.Line + 1, textArea.Caret.Column + 1, ((TextEditorControlBase)textArea.MotherTextEditorControl).FileName, textArea.Document.TextContent);
		}
		return null;
	}
}
