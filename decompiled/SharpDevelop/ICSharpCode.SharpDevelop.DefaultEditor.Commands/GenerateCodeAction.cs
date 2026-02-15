using System.Collections;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class GenerateCodeAction : AbstractMenuCommand
{
	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null || !(activeWorkbenchWindow.ViewContent is ITextEditorControlProvider))
		{
			return;
		}
		TextEditorControl textEditorControl = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
		ParseInformation parseInformation = ((!activeWorkbenchWindow.ViewContent.IsUntitled) ? ParserService.GetParseInformation(textEditorControl.FileName) : ParserService.ParseFile(textEditorControl.FileName, textEditorControl.Document.TextContent));
		if (parseInformation == null)
		{
			return;
		}
		ICompilationUnit mostRecentCompilationUnit = parseInformation.MostRecentCompilationUnit;
		if (mostRecentCompilationUnit == null)
		{
			return;
		}
		IClass currentClass = GetCurrentClass(textEditorControl, mostRecentCompilationUnit, textEditorControl.FileName);
		if (currentClass == null)
		{
			return;
		}
		new ArrayList();
		ArrayList arrayList = AddInTree.BuildItems("/AddIns/DefaultTextEditor/CodeGenerator", this, throwOnNotFound: true);
		using CodeGenerationForm codeGenerationForm = new CodeGenerationForm(textEditorControl, (CodeGeneratorBase[])arrayList.ToArray(typeof(CodeGeneratorBase)), currentClass);
		codeGenerationForm.ShowDialog(WorkbenchSingleton.MainForm);
	}

	private IClass GetCurrentClass(TextEditorControl textEditorControl, ICompilationUnit cu, string fileName)
	{
		IDocument document = textEditorControl.Document;
		if (cu != null)
		{
			int num = document.GetLineNumberForOffset(textEditorControl.ActiveTextAreaControl.Caret.Offset) + 1;
			int column = textEditorControl.ActiveTextAreaControl.Caret.Offset - document.GetLineSegment(num - 1).Offset + 1;
			return FindClass(cu.Classes, num, column);
		}
		return null;
	}

	private IClass FindClass(ICollection classes, int lineNr, int column)
	{
		foreach (IClass @class in classes)
		{
			if (@class.Region.IsInside(lineNr, column))
			{
				IClass obj2 = FindClass(@class.InnerClasses, lineNr, column);
				return (obj2 == null) ? @class : obj2;
			}
		}
		return null;
	}
}
