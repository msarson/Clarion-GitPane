using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.Refactoring;

public abstract class AbstractRefactoringCommand : AbstractMenuCommand
{
	public override void Run()
	{
		if (!ParserService.LoadSolutionProjectsThreadRunning && WorkbenchSingleton.Workbench != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null && WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is ITextEditorControlProvider textEditorControlProvider)
		{
			LanguageProperties language = ParserService.CurrentProjectContent.Language;
			if (language != null)
			{
				RefactoringProvider refactoringProvider = language.RefactoringProvider;
				Run(textEditorControlProvider.TextEditorControl, refactoringProvider);
				textEditorControlProvider.TextEditorControl.Refresh();
			}
		}
	}

	protected ResolveResult ResolveAtCaret(TextEditorControl textEditor)
	{
		string fileName = textEditor.FileName;
		IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(fileName);
		if (expressionFinder == null)
		{
			return null;
		}
		Caret caret = textEditor.ActiveTextAreaControl.Caret;
		string textContent = textEditor.Document.TextContent;
		ExpressionResult expressionResult = expressionFinder.FindFullExpression(textContent, caret.Offset);
		if (expressionResult.Expression == null)
		{
			return null;
		}
		return ParserService.Resolve(expressionResult, caret.Line + 1, caret.Column + 1, fileName, textContent);
	}

	protected abstract void Run(TextEditorControl textEditor, RefactoringProvider provider);
}
