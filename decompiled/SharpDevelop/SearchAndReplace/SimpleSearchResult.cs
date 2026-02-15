using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class SimpleSearchResult : SearchResult
{
	private TextLocation position;

	private string displayText;

	public override string DisplayText => displayText;

	public override TextLocation GetStartPosition(IDocument doc)
	{
		return position;
	}

	public override TextLocation GetEndPosition(IDocument doc)
	{
		return position;
	}

	public SimpleSearchResult(string displayText, TextLocation position)
		: base(0, 0)
	{
		this.position = position;
		this.displayText = displayText;
	}
}
