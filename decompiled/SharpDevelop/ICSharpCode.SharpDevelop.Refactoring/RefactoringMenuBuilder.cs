using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Refactoring;

public class RefactoringMenuBuilder : ISubmenuBuilder
{
	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		TextEditorControl textEditorControl = (TextEditorControl)owner;
		if (textEditorControl.FileName == null)
		{
			return new ToolStripItem[0];
		}
		List<ToolStripItem> list = new List<ToolStripItem>();
		TextArea textArea = textEditorControl.ActiveTextAreaControl.TextArea;
		IDocument document = textArea.Document;
		int line = textArea.Caret.Line;
		List<string> list2 = new List<string>();
		ToolStripMenuItem toolStripMenuItem;
		foreach (Bookmark mark in document.BookmarkManager.Marks)
		{
			if (mark != null && mark.LineNumber == line)
			{
				ClassMemberBookmark classMemberBookmark = mark as ClassMemberBookmark;
				ClassBookmark classBookmark = mark as ClassBookmark;
				IClass obj = null;
				if (classMemberBookmark != null)
				{
					list2.Add(classMemberBookmark.Member.DotNetName);
					toolStripMenuItem = new ToolStripMenuItem(MemberNode.GetText(classMemberBookmark.Member), ClassBrowserIconService.ImageList.Images[classMemberBookmark.IconIndex]);
					MenuService.AddItemsToMenu(toolStripMenuItem.DropDown.Items, mark, "/SharpDevelop/ViewContent/DefaultTextEditor/ClassMemberContextMenu");
					list.Add(toolStripMenuItem);
					obj = classMemberBookmark.Member.DeclaringType;
				}
				else if (classBookmark != null)
				{
					obj = classBookmark.Class;
				}
				if (obj != null)
				{
					list2.Add(obj.DotNetName);
					toolStripMenuItem = new ToolStripMenuItem(obj.Name, ClassBrowserIconService.ImageList.Images[ClassBrowserIconService.GetIcon(obj)]);
					MenuService.AddItemsToMenu(toolStripMenuItem.DropDown.Items, classBookmark ?? new ClassBookmark(textArea.Document, obj), "/SharpDevelop/ViewContent/DefaultTextEditor/ClassBookmarkContextMenu");
					list.Add(toolStripMenuItem);
				}
			}
		}
		IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(textEditorControl.FileName);
		int index = list.Count;
		ExpressionResult expr = FindFullExpressionAtCaret(textArea, expressionFinder);
		ResolveResult resolveResult;
		while (true)
		{
			resolveResult = ResolveExpressionAtCaret(textArea, expr);
			toolStripMenuItem = null;
			if (resolveResult is MethodResolveResult)
			{
				toolStripMenuItem = MakeItem(list2, ((MethodResolveResult)resolveResult).GetMethodIfSingleOverload());
				break;
			}
			if (resolveResult is MemberResolveResult)
			{
				MemberResolveResult memberResolveResult = (MemberResolveResult)resolveResult;
				toolStripMenuItem = MakeItem(list2, memberResolveResult.ResolvedMember);
				if (!RefactoringService.FixIndexerExpression(expressionFinder, ref expr, memberResolveResult))
				{
					break;
				}
				if (toolStripMenuItem != null)
				{
					list.Insert(index, toolStripMenuItem);
				}
				continue;
			}
			if (resolveResult is TypeResolveResult)
			{
				toolStripMenuItem = MakeItem(list2, ((TypeResolveResult)resolveResult).ResolvedClass);
			}
			else if (resolveResult is LocalResolveResult)
			{
				toolStripMenuItem = MakeItem((LocalResolveResult)resolveResult, line + 1 == ((LocalResolveResult)resolveResult).Field.Region.BeginLine);
				index = 0;
			}
			break;
		}
		if (toolStripMenuItem != null)
		{
			list.Insert(index, toolStripMenuItem);
		}
		IMember member = null;
		if (resolveResult != null)
		{
			member = resolveResult.CallingMember;
		}
		else
		{
			ParseInformation parseInformation = ParserService.GetParseInformation(textEditorControl.FileName);
			if (parseInformation != null)
			{
				ICompilationUnit mostRecentCompilationUnit = parseInformation.MostRecentCompilationUnit;
				if (mostRecentCompilationUnit != null)
				{
					IClass innermostClass = mostRecentCompilationUnit.GetInnermostClass(line + 1, textArea.Caret.Column + 1);
					member = GetCallingMember(innermostClass, line + 1, textArea.Caret.Column + 1);
				}
			}
		}
		if (member != null)
		{
			toolStripMenuItem = MakeItem(list2, member);
			if (toolStripMenuItem != null)
			{
				toolStripMenuItem.Text = StringParser.Parse("${res:SharpDevelop.Refactoring.CurrentMethod}: ") + member.Name;
				list.Add(toolStripMenuItem);
			}
		}
		if (list.Count == 0)
		{
			return new ToolStripItem[0];
		}
		list.Add(new MenuSeparator());
		return list.ToArray();
	}

	private IMember GetCallingMember(IClass callingClass, int caretLine, int caretColumn)
	{
		if (callingClass == null)
		{
			return null;
		}
		foreach (IMethod method in callingClass.Methods)
		{
			if (method.BodyRegion.IsInside(caretLine, caretColumn))
			{
				return method;
			}
		}
		foreach (IProperty property in callingClass.Properties)
		{
			if (property.BodyRegion.IsInside(caretLine, caretColumn))
			{
				return property;
			}
		}
		return null;
	}

	private ToolStripMenuItem MakeItem(LocalResolveResult local, bool isDefinition)
	{
		ToolStripMenuItem toolStripMenuItem = MakeItemInternal(MemberNode.GetText(local.Field), local.IsParameter ? 17 : 16, local.CallingClass.CompilationUnit, isDefinition ? DomRegion.Empty : local.Field.Region);
		string text = "/SharpDevelop/ViewContent/DefaultTextEditor/Refactoring/";
		text += (local.IsParameter ? "Parameter" : "LocalVariable");
		if (isDefinition)
		{
			text += "Definition";
		}
		MenuService.AddItemsToMenu(toolStripMenuItem.DropDown.Items, local, text);
		return toolStripMenuItem;
	}

	private ToolStripMenuItem MakeItem(List<string> definitions, IMember member)
	{
		if (member == null)
		{
			return null;
		}
		if (definitions.Contains(member.DotNetName))
		{
			return null;
		}
		definitions.Add(member.DotNetName);
		ToolStripMenuItem toolStripMenuItem = MakeItem(member.FullyQualifiedName, MemberNode.Create(member), member.DeclaringType.CompilationUnit, member.Region);
		ToolStripMenuItem toolStripMenuItem2 = MakeItem(null, member.DeclaringType);
		if (toolStripMenuItem2 != null)
		{
			toolStripMenuItem.DropDown.Items.Add(new ToolStripSeparator());
			toolStripMenuItem2.Text = StringParser.Parse("${res:SharpDevelop.Refactoring.DeclaringType}: ") + toolStripMenuItem2.Text;
			toolStripMenuItem.DropDown.Items.Add(toolStripMenuItem2);
		}
		return toolStripMenuItem;
	}

	private ToolStripMenuItem MakeItem(List<string> definitions, IClass c)
	{
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
		return MakeItem(c.FullyQualifiedName, new ClassNode((IProject)c.ProjectContent.Project, c), c.CompilationUnit, c.Region);
	}

	private ToolStripMenuItem MakeItemInternal(string title, int imageIndex, ICompilationUnit cu, DomRegion region)
	{
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(title, ClassBrowserIconService.ImageList.Images[imageIndex]);
		if (cu.FileName != null && !region.IsEmpty)
		{
			ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem(StringParser.Parse("${res:ICSharpCode.NAntAddIn.GotoDefinitionMenuLabel}"), ClassBrowserIconService.ImageList.Images[13]);
			toolStripMenuItem2.ShortcutKeys = Keys.Return | Keys.Control;
			toolStripMenuItem2.Click += delegate
			{
				FileService.JumpToFilePosition(cu.FileName, region.BeginLine - 1, region.BeginColumn - 1);
			};
			toolStripMenuItem.DropDown.Items.Add(toolStripMenuItem2);
			toolStripMenuItem.DropDown.Items.Add(new ToolStripSeparator());
		}
		return toolStripMenuItem;
	}

	private ToolStripMenuItem MakeItem(string title, ExtTreeNode classBrowserTreeNode, ICompilationUnit cu, DomRegion region)
	{
		ToolStripMenuItem toolStripMenuItem = MakeItemInternal(classBrowserTreeNode.Text, classBrowserTreeNode.ImageIndex, cu, region);
		MenuService.AddItemsToMenu(toolStripMenuItem.DropDown.Items, classBrowserTreeNode, classBrowserTreeNode.ContextmenuAddinTreePath);
		return toolStripMenuItem;
	}

	private static ExpressionResult FindFullExpressionAtCaret(TextArea textArea, IExpressionFinder expressionFinder)
	{
		return expressionFinder?.FindFullExpression(textArea.Document.TextContent, textArea.Caret.Offset) ?? new ExpressionResult(null);
	}

	private static ResolveResult ResolveExpressionAtCaret(TextArea textArea, ExpressionResult expressionResult)
	{
		if (expressionResult.Expression != null)
		{
			return ParserService.Resolve(expressionResult, textArea.Caret.Line + 1, textArea.Caret.Column + 1, textArea.MotherTextEditorControl.FileName, textArea.Document.TextContent);
		}
		return null;
	}
}
