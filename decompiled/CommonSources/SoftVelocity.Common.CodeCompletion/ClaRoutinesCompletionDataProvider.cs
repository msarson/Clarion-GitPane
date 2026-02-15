using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using SoftVelocity.Common.ClassBrowser;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaRoutinesCompletionDataProvider : ClaAbstractCodeCompletionDataProvider
{
	public ClaRoutinesCompletionDataProvider(bool mergeOverriddenMethods)
		: base(mergeOverriddenMethods)
	{
	}

	public ClaRoutinesCompletionDataProvider(bool mergeOverriddenMethods, ExpressionResult expression)
		: base(mergeOverriddenMethods, expression)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


	protected override void GenerateCompletionData(TextArea textArea, char charTyped)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		string textContent = textArea.Document.TextContent;
		ExpressionResult expression = ((AbstractCodeCompletionDataProvider)this).GetExpression(textArea);
		if (expression.Expression != null)
		{
			CodeCompletionTagInfo codeCompletionTagInfo = new CodeCompletionTagInfo();
			codeCompletionTagInfo.IsRoutine = true;
			((ExpressionResult)(ref expression))._002Ector("do", (object)codeCompletionTagInfo);
			((AbstractCodeCompletionDataProvider)this).AddResolveResults(ParserService.Resolve(expression, ((AbstractCodeCompletionDataProvider)this).caretLineNumber, ((AbstractCodeCompletionDataProvider)this).caretColumn, ((AbstractCodeCompletionDataProvider)this).fileName, textContent), expression.Context);
		}
	}

	protected override CodeCompletionData CreateItem(object o, ExpressionContext context)
	{
		if (o is string)
		{
			return (CodeCompletionData)(object)new ClaCodeCompletionData(ta, o.ToString(), ClaClassNode.RoutineIcon);
		}
		return base.CreateItem(o, context);
	}
}
