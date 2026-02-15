using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class CtrlSpaceCompletionDataProvider : CodeCompletionDataProvider
{
	private bool forceNewExpression;

	public bool ForceNewExpression
	{
		get
		{
			return forceNewExpression;
		}
		set
		{
			forceNewExpression = value;
		}
	}

	public CtrlSpaceCompletionDataProvider()
	{
	}

	public CtrlSpaceCompletionDataProvider(ExpressionContext overrideContext)
	{
		base.overrideContext = overrideContext;
	}

	protected override void GenerateCompletionData(TextArea textArea, char charTyped)
	{
		if (forceNewExpression)
		{
			preSelection = "";
			if (charTyped != 0)
			{
				preSelection = null;
			}
			ExpressionContext expressionContext = overrideContext;
			if (expressionContext == null)
			{
				expressionContext = ExpressionContext.Default;
			}
			AddResolveResults(ParserService.CtrlSpace(caretLineNumber, caretColumn, fileName, textArea.Document.TextContent, expressionContext), expressionContext);
			return;
		}
		ExpressionResult expression = GetExpression(textArea);
		string expression2 = expression.Expression;
		preSelection = null;
		if (expression2 == null || expression2.Length == 0)
		{
			preSelection = "";
			if (charTyped != 0)
			{
				preSelection = null;
			}
			AddResolveResults(ParserService.CtrlSpace(caretLineNumber, caretColumn, fileName, textArea.Document.TextContent, expression.Context), expression.Context);
			return;
		}
		int num = expression2.LastIndexOf('.');
		if (num > 0)
		{
			preSelection = expression2.Substring(num + 1);
			expression.Expression = expression2.Substring(0, num);
			if (charTyped != 0)
			{
				preSelection = null;
			}
			GenerateCompletionData(textArea, expression);
		}
		else
		{
			preSelection = expression2;
			if (charTyped != 0)
			{
				preSelection = null;
			}
			AddResolveResults(ParserService.CtrlSpace(caretLineNumber, caretColumn, fileName, textArea.Document.TextContent, expression.Context), expression.Context);
		}
	}
}
