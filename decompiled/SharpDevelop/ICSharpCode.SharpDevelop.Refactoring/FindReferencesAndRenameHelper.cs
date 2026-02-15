using System;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SearchAndReplace;

namespace ICSharpCode.SharpDevelop.Refactoring;

public static class FindReferencesAndRenameHelper
{
	public struct Modification
	{
		public IDocument Document;

		public int Offset;

		public int LengthDifference;

		public Modification(IDocument document, int offset, int lengthDifference)
		{
			Document = document;
			Offset = offset;
			LengthDifference = lengthDifference;
		}
	}

	public static void RenameClass(IClass c)
	{
		string text = MessageService.ShowInputBox("${res:SharpDevelop.Refactoring.Rename}", "${res:SharpDevelop.Refactoring.RenameClassText}", c.Name);
		if (!CheckName(c.ProjectContent.Language, text, c.Name))
		{
			return;
		}
		using (new ProgressNotificationTaskInstance("${res:SharpDevelop.Refactoring.Rename}"))
		{
			RenameClass(c, text);
		}
	}

	public static void RenameClass(IClass c, string newName)
	{
		c = c.GetCompoundClass();
		List<Reference> list = RefactoringService.FindReferences(c, null);
		if (list == null)
		{
			return;
		}
		foreach (IClass classPart in GetClassParts(c))
		{
			AddDeclarationAsReference(list, classPart.CompilationUnit.FileName, classPart.Region, classPart.Name);
		}
		foreach (IMethod method in c.Methods)
		{
			if (method.IsConstructor)
			{
				AddDeclarationAsReference(list, method.DeclaringType.CompilationUnit.FileName, method.Region, c.Name);
			}
		}
		RenameReferences(list, newName);
	}

	private static IList<IClass> GetClassParts(IClass c)
	{
		if (c is CompoundClass compoundClass)
		{
			return compoundClass.GetParts();
		}
		return new IClass[1] { c };
	}

	private static void AddDeclarationAsReference(List<Reference> list, string fileName, DomRegion region, string name)
	{
		if (fileName == null)
		{
			return;
		}
		ProvidedDocumentInformation documentInformation = GetDocumentInformation(fileName);
		int num = documentInformation.CreateDocument().PositionToOffset(new TextLocation(region.BeginColumn - 1, region.BeginLine - 1));
		string text = documentInformation.TextBuffer.GetText(num, Math.Min(name.Length + 30, documentInformation.TextBuffer.Length - num - 1));
		int num2 = -1;
		do
		{
			num2 = text.IndexOf(name, num2 + 1);
			if (num2 < 0 || num2 >= text.Length)
			{
				return;
			}
		}
		while (num2 + name.Length < text.Length && char.IsLetterOrDigit(text[num2 + name.Length]));
		num += num2;
		foreach (Reference item in list)
		{
			if (item.Offset == num)
			{
				return;
			}
		}
		list.Add(new Reference(fileName, num, name.Length, name, null));
	}

	public static void RenameMember(IMember member)
	{
		if (member.DeclaringType != null)
		{
			string text = MessageService.ShowInputBox("${res:SharpDevelop.Refactoring.Rename}", "${res:SharpDevelop.Refactoring.RenameMemberText}", member.Name);
			if (CheckName(member.DeclaringType.ProjectContent.Language, text, member.Name))
			{
				RenameMember(member, text);
			}
		}
	}

	public static bool RenameMember(IMember member, string newName)
	{
		using (ProgressNotificationTaskInstance progressMonitor = new ProgressNotificationTaskInstance("${res:SharpDevelop.Refactoring.Rename}"))
		{
			List<Reference> list = RefactoringService.FindReferences(member, progressMonitor);
			if (list == null)
			{
				return false;
			}
			RenameReferences(list, newName);
		}
		if (member is IField)
		{
			IProperty property = FindProperty((IField)member);
			if (property != null)
			{
				string propertyName = member.DeclaringType.ProjectContent.Language.CodeGenerator.GetPropertyName(newName);
				if (propertyName != newName && propertyName != property.Name && MessageService.AskQuestionFormatted("${res:SharpDevelop.Refactoring.Rename}", "${res:SharpDevelop.Refactoring.RenameFieldAndProperty}", property.FullyQualifiedName, propertyName))
				{
					using ProgressNotificationTaskInstance progressMonitor2 = new ProgressNotificationTaskInstance("${res:SharpDevelop.Refactoring.Rename}");
					List<Reference> list = RefactoringService.FindReferences(property, progressMonitor2);
					if (list != null)
					{
						RenameReferences(list, propertyName);
					}
				}
			}
		}
		return true;
	}

	internal static IProperty FindProperty(IField field)
	{
		LanguageProperties language = field.DeclaringType.ProjectContent.Language;
		if (language.CodeGenerator == null)
		{
			return null;
		}
		string propertyName = language.CodeGenerator.GetPropertyName(field.Name);
		IProperty result = null;
		foreach (IProperty property in field.DeclaringType.Properties)
		{
			if (language.NameComparer.Equals(propertyName, property.Name))
			{
				result = property;
				break;
			}
		}
		return result;
	}

	public static ProvidedDocumentInformation GetDocumentInformation(string fileName)
	{
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			if (item is ITextEditorControlProvider && FileUtility.IsEqualFileName(item.IsUntitled ? item.UntitledName : item.FileName, fileName))
			{
				return new ProvidedDocumentInformation(((ITextEditorControlProvider)item).TextEditorControl.Document, fileName, 0);
			}
		}
		ITextBufferStrategy textBuffer = StringTextBufferStrategy.CreateTextBufferFromFile(fileName);
		return new ProvidedDocumentInformation(textBuffer, fileName, 0);
	}

	public static bool IsReadOnly(IClass c)
	{
		if (c.CompilationUnit.FileName != null)
		{
			return c.GetCompoundClass().IsSynthetic;
		}
		return true;
	}

	public static TextEditorControl JumpToDefinition(IMember member)
	{
		IViewContent viewContent = null;
		ICompilationUnit compilationUnit = member.DeclaringType.CompilationUnit;
		if (compilationUnit != null)
		{
			string fileName = compilationUnit.FileName;
			if (fileName != null)
			{
				if (!member.Region.IsEmpty)
				{
					viewContent = FileService.JumpToFilePosition(fileName, member.Region.BeginLine - 1, member.Region.BeginColumn - 1);
				}
				else
				{
					FileService.OpenFile(fileName);
				}
			}
		}
		if (viewContent is ITextEditorControlProvider textEditorControlProvider)
		{
			return textEditorControlProvider.TextEditorControl;
		}
		return null;
	}

	public static TextEditorControl JumpBehindDefinition(IMember member)
	{
		IViewContent viewContent = null;
		ICompilationUnit compilationUnit = member.DeclaringType.CompilationUnit;
		if (compilationUnit != null)
		{
			string fileName = compilationUnit.FileName;
			if (fileName != null)
			{
				if (!member.Region.IsEmpty)
				{
					viewContent = FileService.JumpToFilePosition(fileName, member.Region.EndLine, 0);
				}
				else
				{
					FileService.OpenFile(fileName);
				}
			}
		}
		if (viewContent is ITextEditorControlProvider textEditorControlProvider)
		{
			return textEditorControlProvider.TextEditorControl;
		}
		return null;
	}

	public static bool CheckName(LanguageProperties language, string name, string oldName)
	{
		if (name == null || name.Length == 0 || name == oldName)
		{
			return false;
		}
		string error = string.Empty;
		if (!language.CheckName(name, ref error))
		{
			if (string.IsNullOrEmpty(error))
			{
				MessageService.ShowError(error);
			}
			return false;
		}
		return true;
	}

	public static void ModifyDocument(List<Modification> modifications, IDocument doc, int offset, int length, string newName)
	{
		foreach (Modification modification in modifications)
		{
			if (modification.Document == doc && modification.Offset < offset)
			{
				offset += modification.LengthDifference;
			}
		}
		int num = newName.Length - length;
		doc.Replace(offset, length, newName);
		if (num == 0)
		{
			return;
		}
		for (int i = 0; i < modifications.Count; i++)
		{
			Modification value = modifications[i];
			if (value.Document == doc && value.Offset > offset)
			{
				value.Offset += num;
				modifications[i] = value;
			}
		}
		modifications.Add(new Modification(doc, offset, num));
	}

	public static void ShowAsSearchResults(string pattern, List<Reference> list)
	{
		if (list == null)
		{
			return;
		}
		List<SearchResult> list2 = new List<SearchResult>(list.Count);
		foreach (Reference item in list)
		{
			SearchResult searchResult = new SearchResult(item.Offset, item.Length);
			searchResult.ProvidedDocumentInformation = GetDocumentInformation(item.FileName);
			list2.Add(searchResult);
		}
		SearchInFilesManager.ShowSearchResults(pattern, list2);
	}

	public static void RenameReferences(List<Reference> list, string newName)
	{
		List<IViewContent> list2 = new List<IViewContent>();
		List<Modification> modifications = new List<Modification>();
		IWorkbench workbench = WorkbenchSingleton.Workbench;
		foreach (Reference item in list)
		{
			IViewContent viewContent = FileService.OpenFile(item.FileName).ViewContent;
			if (!list2.Contains(viewContent))
			{
				list2.Add(viewContent);
			}
			if (viewContent is ITextEditorControlProvider textEditorControlProvider)
			{
				ModifyDocument(modifications, textEditorControlProvider.TextEditorControl.Document, item.Offset, item.Length, newName);
			}
		}
		workbench?.ActiveWorkbenchWindow.SelectWindow();
		foreach (IViewContent item2 in list2)
		{
			ParserService.ParseViewContent(item2);
		}
	}
}
