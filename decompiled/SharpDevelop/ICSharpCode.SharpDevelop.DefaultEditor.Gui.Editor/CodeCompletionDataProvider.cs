using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class CodeCompletionDataProvider : AbstractCodeCompletionDataProvider
{
	protected ExpressionResult fixedExpression;

	public CodeCompletionDataProvider()
	{
	}

	public CodeCompletionDataProvider(ExpressionResult expression)
	{
		fixedExpression = expression;
	}

	protected override void GenerateCompletionData(TextArea textArea, char charTyped)
	{
		preSelection = null;
		if (fixedExpression.Expression == null)
		{
			GenerateCompletionData(textArea, GetExpression(textArea));
		}
		else
		{
			GenerateCompletionData(textArea, fixedExpression);
		}
	}

	protected void GenerateCompletionData(TextArea textArea, ExpressionResult expressionResult)
	{
		if (expressionResult.Expression == null)
		{
			return;
		}
		if (LoggingService.IsDebugEnabled)
		{
			if (expressionResult.Context == ExpressionContext.Default)
			{
				LoggingService.DebugFormatted("GenerateCompletionData for >>{0}<<", expressionResult.Expression);
			}
			else
			{
				LoggingService.DebugFormatted("GenerateCompletionData for >>{0}<<, context={1}", expressionResult.Expression, expressionResult.Context);
			}
		}
		string textContent = textArea.Document.TextContent;
		AddResolveResults(ParserService.Resolve(expressionResult, caretLineNumber, caretColumn, fileName, textContent), expressionResult.Context);
	}
}
