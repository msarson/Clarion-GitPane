using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class IndexerInsightDataProvider : MethodInsightDataProvider
{
	public IndexerInsightDataProvider()
	{
	}

	public IndexerInsightDataProvider(int lookupOffset, bool setupOnlyOnce)
		: base(lookupOffset, setupOnlyOnce)
	{
	}

	protected override void SetupDataProvider(string fileName, IDocument document, ExpressionResult expressionResult, int caretLineNumber, int caretColumn)
	{
		ResolveResult resolveResult = ParserService.Resolve(expressionResult, caretLineNumber, caretColumn, fileName, document.TextContent);
		if (resolveResult == null)
		{
			return;
		}
		IReturnType resolvedType = resolveResult.ResolvedType;
		if (resolvedType == null)
		{
			return;
		}
		foreach (IProperty property in resolvedType.GetProperties())
		{
			if (property.IsIndexer)
			{
				methods.Add(property);
			}
		}
	}
}
