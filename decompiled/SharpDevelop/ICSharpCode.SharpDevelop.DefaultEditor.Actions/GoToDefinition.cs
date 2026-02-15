using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Actions;

public class GoToDefinition : AbstractEditAction
{
	public override void Execute(TextArea textArea)
	{
		TextEditorControl motherTextEditorControl = textArea.MotherTextEditorControl;
		IDocument document = motherTextEditorControl.Document;
		string textContent = document.TextContent;
		int num = document.GetLineNumberForOffset(motherTextEditorControl.ActiveTextAreaControl.Caret.Offset) + 1;
		int caretColumn = motherTextEditorControl.ActiveTextAreaControl.Caret.Offset - document.GetLineSegment(num - 1).Offset + 1;
		IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(motherTextEditorControl.FileName);
		if (expressionFinder == null)
		{
			return;
		}
		ExpressionResult expressionResult = expressionFinder.FindFullExpression(textContent, motherTextEditorControl.ActiveTextAreaControl.Caret.Offset);
		if (expressionResult.Expression == null || expressionResult.Expression.Length == 0)
		{
			return;
		}
		ResolveResult resolveResult = ParserService.Resolve(expressionResult, num, caretColumn, motherTextEditorControl.FileName, textContent);
		if (resolveResult == null)
		{
			return;
		}
		FilePosition definitionPosition = resolveResult.GetDefinitionPosition();
		if (definitionPosition.IsEmpty)
		{
			return;
		}
		try
		{
			if (definitionPosition.Position.IsEmpty)
			{
				FileService.OpenFile(definitionPosition.FileName);
			}
			else
			{
				FileService.JumpToFilePosition(definitionPosition.FileName, definitionPosition.Line - 1, definitionPosition.Column - 1);
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex, "Error jumping to '" + definitionPosition.FileName + "'.");
		}
	}
}
