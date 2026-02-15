using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaDotCodeCompletionDataProvider : ClaAbstractCodeCompletionDataProvider
{
	public ClaDotCodeCompletionDataProvider(bool mergeOverriddenMethods)
		: base(mergeOverriddenMethods)
	{
	}

	public ClaDotCodeCompletionDataProvider(bool mergeOverriddenMethods, ExpressionResult expression)
		: base(mergeOverriddenMethods, expression)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


	protected override void GenerateCompletionData(TextArea textArea, char charTyped)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		((AbstractCompletionDataProvider)this).preSelection = null;
		if (((CodeCompletionDataProvider)this).fixedExpression.Expression == null)
		{
			ExpressionResult expression = ((AbstractCodeCompletionDataProvider)this).GetExpression(textArea);
			CodeCompletionTagInfo codeCompletionTagInfo = new CodeCompletionTagInfo();
			codeCompletionTagInfo.IsDot = true;
			expression.Tag = codeCompletionTagInfo;
			if (expression.Expression != null && textArea.Caret.Column == expression.Expression.Length)
			{
				codeCompletionTagInfo.IsLabelExpression = true;
			}
			((CodeCompletionDataProvider)this).GenerateCompletionData(textArea, expression);
		}
		else
		{
			((CodeCompletionDataProvider)this).GenerateCompletionData(textArea, ((CodeCompletionDataProvider)this).fixedExpression);
		}
	}
}
