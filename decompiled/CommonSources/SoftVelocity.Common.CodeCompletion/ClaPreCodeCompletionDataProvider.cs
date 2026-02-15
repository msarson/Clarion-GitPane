using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaPreCodeCompletionDataProvider : ClaAbstractCodeCompletionDataProvider
{
	public ClaPreCodeCompletionDataProvider(bool mergeOverriddenMethods)
		: base(mergeOverriddenMethods)
	{
	}

	public ClaPreCodeCompletionDataProvider(bool mergeOverriddenMethods, ExpressionResult expression)
		: base(mergeOverriddenMethods, expression)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


	protected override void GenerateCompletionData(TextArea textArea, char charTyped)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		((AbstractCompletionDataProvider)this).preSelection = null;
		if (((CodeCompletionDataProvider)this).fixedExpression.Expression == null)
		{
			ExpressionResult expression = ((AbstractCodeCompletionDataProvider)this).GetExpression(textArea);
			CodeCompletionTagInfo codeCompletionTagInfo = new CodeCompletionTagInfo();
			codeCompletionTagInfo.IsPre = true;
			expression.Tag = codeCompletionTagInfo;
			((CodeCompletionDataProvider)this).GenerateCompletionData(textArea, expression);
		}
		else
		{
			((CodeCompletionDataProvider)this).GenerateCompletionData(textArea, ((CodeCompletionDataProvider)this).fixedExpression);
		}
	}
}
