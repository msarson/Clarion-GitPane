using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Refactoring;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SearchAndReplace;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ClassBookmarkMenuBuilder : ISubmenuBuilder
{
	private static IClass GetPart(IClass possibleCompound, string fileName)
	{
		if (!(possibleCompound is CompoundClass compoundClass))
		{
			return possibleCompound;
		}
		IList<IClass> parts = compoundClass.GetParts();
		if (!string.IsNullOrEmpty(fileName))
		{
			foreach (IClass item in parts)
			{
				if (FileUtility.IsEqualFileName(fileName, item.CompilationUnit.FileName))
				{
					return item;
				}
			}
		}
		IClass obj = parts[0];
		for (int i = 1; i < parts.Count; i++)
		{
			if (IsShorterFileName(parts[i].CompilationUnit.FileName, obj.CompilationUnit.FileName))
			{
				obj = parts[i];
			}
		}
		return obj;
	}

	private static bool IsShorterFileName(string a, string b)
	{
		if (a == null)
		{
			return false;
		}
		if (b == null)
		{
			return true;
		}
		return a.Length < b.Length;
	}

	private static IClass GetCurrentPart(IClass possibleCompound)
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null)
		{
			return GetPart(possibleCompound, activeWorkbenchWindow.ViewContent.FileName);
		}
		return GetPart(possibleCompound, null);
	}

	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		IClass c;
		if (owner is ClassNode classNode)
		{
			c = classNode.Class;
		}
		else
		{
			ClassBookmark classBookmark = (ClassBookmark)owner;
			c = classBookmark.Class;
		}
		ParserService.ParseCurrentViewContent();
		c = c.ProjectContent.GetClass(c.FullyQualifiedName, c.TypeParameters.Count);
		c = GetCurrentPart(c);
		if (c == null)
		{
			return new ToolStripMenuItem[0];
		}
		LanguageProperties language = c.ProjectContent.Language;
		List<ToolStripItem> list = new List<ToolStripItem>();
		MenuCommand item;
		if (!FindReferencesAndRenameHelper.IsReadOnly(c))
		{
			if (c.DeclaringType == null && !c.BodyRegion.IsEmpty && !c.Name.Equals(Path.GetFileNameWithoutExtension(c.CompilationUnit.FileName), StringComparison.InvariantCultureIgnoreCase))
			{
				string correctFileName = Path.Combine(Path.GetDirectoryName(c.CompilationUnit.FileName), c.Name + Path.GetExtension(c.CompilationUnit.FileName));
				if (FileUtility.IsValidFileName(correctFileName) && Path.IsPathRooted(correctFileName) && !File.Exists(correctFileName))
				{
					if (c.CompilationUnit.Classes.Count == 1)
					{
						item = new MenuCommand(StringParser.Parse("${res:SharpDevelop.Refactoring.RenameFileTo}", new string[1, 2] { 
						{
							"FileName",
							Path.GetFileName(correctFileName)
						} }), delegate
						{
							FileService.RenameFile(c.CompilationUnit.FileName, correctFileName, isDirectory: false);
							if (c.ProjectContent.Project != null)
							{
								((IProject)c.ProjectContent.Project).Save();
							}
						});
						list.Add(item);
					}
					else if (language.RefactoringProvider.SupportsCreateNewFileLikeExisting && language.RefactoringProvider.SupportsGetFullCodeRangeForType)
					{
						item = new MenuCommand(StringParser.Parse("${res:SharpDevelop.Refactoring.MoveClassToFile}", new string[1, 2] { 
						{
							"FileName",
							Path.GetFileName(correctFileName)
						} }), delegate
						{
							MoveClassToFile(c, correctFileName);
						});
						list.Add(item);
					}
				}
			}
			item = new MenuCommand("${res:SharpDevelop.Refactoring.RenameCommand}", Rename);
			item.Tag = c;
			list.Add(item);
		}
		if (c.BaseTypes.Count > 0)
		{
			list.Add(new MenuSeparator());
			item = ((c.BaseClass == null || c.BaseClass.GetType().ToString().Contains("FakeParentClass")) ? new MenuCommand("Goto Declaration", GoToClaDeclaration) : new MenuCommand("${res:SharpDevelop.Refactoring.GoToBaseCommand}", GoToBase));
			item.Tag = c;
			list.Add(item);
			if (c.ClassType != ClassType.Interface && !FindReferencesAndRenameHelper.IsReadOnly(c))
			{
				AddImplementInterfaceCommands(c, list);
			}
		}
		list.Add(new MenuSeparator());
		if (!c.IsSealed && !c.IsStatic)
		{
			item = new MenuCommand("${res:SharpDevelop.Refactoring.FindDerivedClassesCommand}", FindDerivedClasses);
			item.Tag = c;
			list.Add(item);
		}
		item = new MenuCommand("${res:SharpDevelop.Refactoring.FindReferencesCommand}", FindReferences);
		item.Tag = c;
		list.Add(item);
		return list.ToArray();
	}

	private static void MoveClassToFile(IClass c, string newFileName)
	{
		LanguageProperties language = c.ProjectContent.Language;
		string parseableFileContent = ParserService.GetParseableFileContent(c.CompilationUnit.FileName);
		DomRegion fullCodeRangeForType = language.RefactoringProvider.GetFullCodeRangeForType(parseableFileContent, c);
		if (!fullCodeRangeForType.IsEmpty)
		{
			string codeForNewType = ExtractCode(c, fullCodeRangeForType, c.BodyRegion.BeginLine);
			codeForNewType = language.RefactoringProvider.CreateNewFileLikeExisting(parseableFileContent, codeForNewType);
			IWorkbenchWindow workbenchWindow = FileService.NewFile(newFileName, "Text", codeForNewType);
			workbenchWindow.ViewContent.Save(newFileName);
			IProject project = (IProject)c.ProjectContent.Project;
			if (project != null)
			{
				FileProjectItem fileProjectItem = new FileProjectItem(project, ItemType.Compile);
				fileProjectItem.FileName = newFileName;
				ProjectService.AddProjectItem(project, fileProjectItem);
				project.Save();
				ProjectBrowserPad.Instance.ProjectBrowserControl.RefreshView();
			}
		}
	}

	private static string ExtractCode(IClass c, DomRegion codeRegion, int indentationLine)
	{
		ICSharpCode.TextEditor.Document.IDocument document = GetDocument(c);
		if (indentationLine < 1)
		{
			indentationLine = 1;
		}
		if (indentationLine >= document.TotalNumberOfLines)
		{
			indentationLine = document.TotalNumberOfLines;
		}
		LineSegment lineSegment = document.GetLineSegment(indentationLine - 1);
		string text = document.GetText(lineSegment);
		string text2 = text.Substring(0, text.Length - text.TrimStart().Length);
		lineSegment = document.GetLineSegment(codeRegion.BeginLine - 1);
		int offset = lineSegment.Offset;
		lineSegment = document.GetLineSegment(codeRegion.EndLine - 1);
		int num = lineSegment.Offset + lineSegment.Length;
		StringReader stringReader = new StringReader(document.GetText(offset, num - offset));
		document.Remove(offset, num - offset);
		document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
		document.CommitUpdate();
		StringBuilder stringBuilder = new StringBuilder();
		int length = 0;
		string text3;
		while ((text3 = stringReader.ReadLine()) != null)
		{
			int i;
			for (i = 0; i < text3.Length && i < text2.Length && text3[i] == text2[i]; i++)
			{
			}
			if (i == text3.Length)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
			}
			else
			{
				stringBuilder.Append(text3, i, text3.Length - i);
				stringBuilder.AppendLine();
				length = stringBuilder.Length;
			}
		}
		stringBuilder.Length = length;
		return stringBuilder.ToString();
	}

	private void AddImplementInterfaceCommandItems(List<ToolStripItem> subItems, IClass c, bool explicitImpl)
	{
		CodeGenerator codeGen = c.ProjectContent.Language.CodeGenerator;
		IAmbience currentAmbience = AmbienceService.CurrentAmbience;
		currentAmbience.ConversionFlags = ConversionFlags.None;
		foreach (IReturnType baseType in c.BaseTypes)
		{
			IClass underlyingClass = baseType.GetUnderlyingClass();
			if (underlyingClass == null || underlyingClass.ClassType != ClassType.Interface)
			{
				continue;
			}
			IReturnType rtCopy = baseType;
			EventHandler handler = delegate
			{
				TextEditorDocument textEditorDocument = new TextEditorDocument(GetDocument(c));
				if (textEditorDocument != null)
				{
					codeGen.ImplementInterface(rtCopy, textEditorDocument, explicitImpl, c);
				}
				ParserService.ParseCurrentViewContent();
			};
			subItems.Add(new MenuCommand(currentAmbience.Convert(underlyingClass), handler));
		}
	}

	private void AddImplementInterfaceCommands(IClass c, List<ToolStripItem> list)
	{
		CodeGenerator codeGenerator = c.ProjectContent.Language.CodeGenerator;
		if (codeGenerator == null)
		{
			return;
		}
		List<ToolStripItem> list2 = new List<ToolStripItem>();
		if (c.ProjectContent.Language.SupportsImplicitInterfaceImplementation)
		{
			AddImplementInterfaceCommandItems(list2, c, explicitImpl: false);
			if (list2.Count > 0)
			{
				list.Add(new ICSharpCode.Core.Menu("${res:SharpDevelop.Refactoring.ImplementInterfaceImplicit}", list2.ToArray()));
				list2 = new List<ToolStripItem>();
			}
		}
		AddImplementInterfaceCommandItems(list2, c, explicitImpl: true);
		if (list2.Count > 0)
		{
			if (c.ProjectContent.Language.SupportsImplicitInterfaceImplementation)
			{
				list.Add(new ICSharpCode.Core.Menu("${res:SharpDevelop.Refactoring.ImplementInterfaceExplicit}", list2.ToArray()));
			}
			else
			{
				list.Add(new ICSharpCode.Core.Menu("${res:SharpDevelop.Refactoring.ImplementInterface}", list2.ToArray()));
			}
		}
	}

	private static ICSharpCode.TextEditor.Document.IDocument GetDocument(IClass c)
	{
		IWorkbenchWindow workbenchWindow = FileService.OpenFile(c.CompilationUnit.FileName);
		if (workbenchWindow == null)
		{
			return null;
		}
		if (!(workbenchWindow.ViewContent is ITextEditorControlProvider textEditorControlProvider))
		{
			return null;
		}
		return textEditorControlProvider.TextEditorControl.Document;
	}

	private void GoToBase(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		IClass obj = (IClass)menuCommand.Tag;
		IClass baseClass = obj.BaseClass;
		if (baseClass != null && !baseClass.GetType().ToString().Contains("FakeParentClass"))
		{
			string fileName = baseClass.CompilationUnit.FileName;
			if (fileName != null)
			{
				FileService.JumpToFilePosition(fileName, baseClass.Region.BeginLine - 1, baseClass.Region.BeginColumn - 1);
			}
		}
	}

	private void GoToClaDeclaration(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		if (menuCommand != null)
		{
			string pFileName = null;
			int pBeginLine = 0;
			int pBeginColumn = 0;
			if (GetClaRegionFileNameValue(menuCommand.Tag, out pFileName, out pBeginLine, out pBeginColumn))
			{
				FileService.JumpToFilePosition(pFileName, pBeginLine - 1, pBeginColumn - 1);
			}
		}
	}

	private static bool GetClaRegionFileNameValue(object c, out string pFileName, out int pBeginLine, out int pBeginColumn)
	{
		pFileName = "";
		pBeginLine = 0;
		pBeginColumn = 0;
		if (c != null)
		{
			PropertyInfo property = c.GetType().GetProperty("ClaRegion");
			if (property != null)
			{
				object value = property.GetValue(c, null);
				PropertyInfo property2 = value.GetType().GetProperty("FileName");
				PropertyInfo property3 = value.GetType().GetProperty("BeginLine");
				PropertyInfo property4 = value.GetType().GetProperty("BeginColumn");
				if (property2 != null && property3 != null && property4 != null)
				{
					pFileName = property2.GetValue(value, null).ToString();
					pBeginLine = (int)property3.GetValue(value, null);
					pBeginColumn = (int)property4.GetValue(value, null);
					return true;
				}
			}
		}
		return false;
	}

	private static IClass FindProjectClass(IClass ic)
	{
		IClass obj = null;
		IClass obj2 = null;
		using (IEnumerator<IProjectContent> enumerator = ParserService.AllProjectContents.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				IProjectContent current = enumerator.Current;
				foreach (IClass @class in current.Classes)
				{
					if (ic.Name == @class.Name)
					{
						if (obj == null && ic.CompilationUnit.FileName != @class.CompilationUnit.FileName)
						{
							obj = @class;
						}
						obj2 = @class;
					}
				}
			}
		}
		if (obj != null)
		{
			return obj;
		}
		if (obj2 != null)
		{
			return obj2;
		}
		return ic;
	}

	private void Rename(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		FindReferencesAndRenameHelper.RenameClass((IClass)menuCommand.Tag);
	}

	private void FindDerivedClasses(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		IClass obj = (IClass)menuCommand.Tag;
		List<IClass> list = RefactoringService.FindDerivedClasses(obj, ParserService.AllProjectContents, directDerivationOnly: false);
		List<SearchResult> list2 = new List<SearchResult>();
		foreach (IClass item in list)
		{
			if (item.CompilationUnit != null && item.CompilationUnit.FileName != null)
			{
				SearchResult searchResult = new SimpleSearchResult(item.FullyQualifiedName, new TextLocation(item.Region.BeginColumn - 1, item.Region.BeginLine - 1));
				searchResult.ProvidedDocumentInformation = FindReferencesAndRenameHelper.GetDocumentInformation(item.CompilationUnit.FileName);
				list2.Add(searchResult);
			}
		}
		SearchInFilesManager.ShowSearchResults(StringParser.Parse("${res:SharpDevelop.Refactoring.ClassesDerivingFrom}", new string[1, 2] { { "Name", obj.Name } }), list2);
	}

	private void FindReferences(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		IClass obj = (IClass)menuCommand.Tag;
		using ProgressNotificationTaskInstance progressMonitor = new ProgressNotificationTaskInstance("${res:SharpDevelop.Refactoring.FindReferences}");
		FindReferencesAndRenameHelper.ShowAsSearchResults(StringParser.Parse("${res:SharpDevelop.Refactoring.ReferencesTo}", new string[1, 2] { { "Name", obj.Name } }), RefactoringService.FindReferences(obj, progressMonitor));
	}
}
