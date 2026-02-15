using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Refactoring;
using ICSharpCode.TextEditor;
using SearchAndReplace;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ClassMemberMenuBuilder : ISubmenuBuilder
{
	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		IMember member;
		if (owner is MemberNode memberNode)
		{
			member = memberNode.Member;
		}
		else
		{
			ClassMemberBookmark classMemberBookmark = (ClassMemberBookmark)owner;
			member = classMemberBookmark.Member;
		}
		IMethod method = member as IMethod;
		List<ToolStripItem> list = new List<ToolStripItem>();
		bool flag = member.DeclaringType.ProjectContent.Language.CodeGenerator != null && !FindReferencesAndRenameHelper.IsReadOnly(member.DeclaringType);
		MenuCommand menuCommand;
		if ((method == null || !method.IsConstructor) && !FindReferencesAndRenameHelper.IsReadOnly(member.DeclaringType) && (!(member is IProperty) || !((IProperty)member).IsIndexer))
		{
			menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.RenameCommand}", Rename);
			menuCommand.Tag = member;
			list.Add(menuCommand);
		}
		if (member.IsOverride)
		{
			menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.GoToBaseClassCommand}", GoToBase);
			menuCommand.Tag = member;
			list.Add(menuCommand);
		}
		if (member.IsVirtual || member.IsAbstract || (member.IsOverride && !member.DeclaringType.IsSealed))
		{
			menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.FindOverridesCommand}", FindOverrides);
			menuCommand.Tag = member;
			list.Add(menuCommand);
		}
		menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.FindReferencesCommand}", FindReferences);
		menuCommand.Tag = member;
		list.Add(menuCommand);
		if (member is IField && member.DeclaringType.ClassType != ClassType.Enum)
		{
			IProperty property = FindReferencesAndRenameHelper.FindProperty(member as IField);
			if (property != null)
			{
				menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.GoToProperty}", GotoTagMember);
				menuCommand.Tag = property;
				list.Add(menuCommand);
			}
			else if (flag)
			{
				if (member.IsReadonly)
				{
					menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.CreateProperty}", CreateGetter);
					menuCommand.Tag = member;
					list.Add(menuCommand);
				}
				else
				{
					menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.CreateGetter}", CreateGetter);
					menuCommand.Tag = member;
					list.Add(menuCommand);
					menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.CreateProperty}", CreateProperty);
					menuCommand.Tag = member;
					list.Add(menuCommand);
				}
			}
		}
		if (member is IProperty && ((IProperty)member).CanSet && flag)
		{
			menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.CreateChangedEvent}", CreateChangedEvent);
			menuCommand.Tag = member;
			list.Add(menuCommand);
		}
		if (member is IEvent && flag)
		{
			menuCommand = new MenuCommand("${res:SharpDevelop.Refactoring.CreateOnEventMethod}", CreateOnEventMethod);
			menuCommand.Tag = member;
			list.Add(menuCommand);
		}
		return list.ToArray();
	}

	private void CreateProperty(object sender, EventArgs e)
	{
		CreateProperty(sender, e, includeSetter: true);
	}

	private void CreateGetter(object sender, EventArgs e)
	{
		CreateProperty(sender, e, includeSetter: false);
	}

	private void CreateProperty(object sender, EventArgs e, bool includeSetter)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		IField field = (IField)menuCommand.Tag;
		TextEditorControl textEditorControl = FindReferencesAndRenameHelper.JumpBehindDefinition(field);
		CodeGenerator codeGenerator = field.DeclaringType.ProjectContent.Language.CodeGenerator;
		codeGenerator.InsertCodeAfter(field, new TextEditorDocument(textEditorControl.Document), codeGenerator.CreateProperty(field, createGetter: true, includeSetter));
		ParserService.ParseCurrentViewContent();
	}

	private void CreateChangedEvent(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		IProperty property = (IProperty)menuCommand.Tag;
		TextEditorControl textEditorControl = FindReferencesAndRenameHelper.JumpBehindDefinition(property);
		property.DeclaringType.ProjectContent.Language.CodeGenerator.CreateChangedEvent(property, new TextEditorDocument(textEditorControl.Document));
		ParserService.ParseCurrentViewContent();
	}

	private void CreateOnEventMethod(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		IEvent obj = (IEvent)menuCommand.Tag;
		TextEditorControl textEditorControl = FindReferencesAndRenameHelper.JumpBehindDefinition(obj);
		CodeGenerator codeGenerator = obj.DeclaringType.ProjectContent.Language.CodeGenerator;
		codeGenerator.InsertCodeAfter(obj, new TextEditorDocument(textEditorControl.Document), codeGenerator.CreateOnEventMethod(obj));
		ParserService.ParseCurrentViewContent();
	}

	private void GotoTagMember(object sender, EventArgs e)
	{
		FindReferencesAndRenameHelper.JumpToDefinition((IMember)(sender as MenuCommand).Tag);
	}

	private void GoToBase(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		IMember member = (IMember)menuCommand.Tag;
		IMember member2 = RefactoringService.FindBaseMember(member);
		if (member2 != null)
		{
			FindReferencesAndRenameHelper.JumpToDefinition(member2);
		}
	}

	private void Rename(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		FindReferencesAndRenameHelper.RenameMember((IMember)menuCommand.Tag);
	}

	private void FindOverrides(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		IMember member = (IMember)menuCommand.Tag;
		List<IClass> list = RefactoringService.FindDerivedClasses(member.DeclaringType, ParserService.AllProjectContents, directDerivationOnly: false);
		List<SearchResult> list2 = new List<SearchResult>();
		foreach (IClass item in list)
		{
			if (item.CompilationUnit != null && item.CompilationUnit.FileName != null)
			{
				IMember member2 = RefactoringService.FindSimilarMember(item, member);
				if (member2 != null && !member2.Region.IsEmpty)
				{
					SearchResult searchResult = new SimpleSearchResult(member2.FullyQualifiedName, new TextLocation(member2.Region.BeginColumn - 1, member2.Region.BeginLine - 1));
					searchResult.ProvidedDocumentInformation = FindReferencesAndRenameHelper.GetDocumentInformation(item.CompilationUnit.FileName);
					list2.Add(searchResult);
				}
			}
		}
		SearchInFilesManager.ShowSearchResults(StringParser.Parse("${res:SharpDevelop.Refactoring.OverridesOf}", new string[1, 2] { { "Name", member.Name } }), list2);
	}

	private void FindReferences(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		IMember member = (IMember)menuCommand.Tag;
		string text = ((!(member is IProperty) || !((IProperty)member).IsIndexer) ? member.Name : (member.Name + " of " + member.DeclaringType.Name));
		using ProgressNotificationTaskInstance progressMonitor = new ProgressNotificationTaskInstance("${res:SharpDevelop.Refactoring.FindReferences}");
		FindReferencesAndRenameHelper.ShowAsSearchResults(StringParser.Parse("${res:SharpDevelop.Refactoring.ReferencesTo}", new string[1, 2] { { "Name", text } }), RefactoringService.FindReferences(member, progressMonitor));
	}
}
