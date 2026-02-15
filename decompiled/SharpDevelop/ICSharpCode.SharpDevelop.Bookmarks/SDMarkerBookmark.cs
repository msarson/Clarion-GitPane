using System;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public abstract class SDMarkerBookmark : SDBookmark
{
	private IDocument oldDocument;

	private TextMarker oldMarker;

	public SDMarkerBookmark(string fileName, IDocument document, int lineNumber)
		: base(fileName, document, lineNumber)
	{
		SetMarker();
	}

	protected abstract TextMarker CreateMarker();

	private void SetMarker()
	{
		RemoveMarker();
		if (base.Document != null)
		{
			TextMarker textMarker = CreateMarker();
			base.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, base.LineNumber));
			base.Document.CommitUpdate();
			oldMarker = textMarker;
		}
		oldDocument = base.Document;
	}

	protected override void OnDocumentChanged(EventArgs e)
	{
		base.OnDocumentChanged(e);
		SetMarker();
	}

	public void RemoveMarker()
	{
		if (oldDocument != null)
		{
			int startLine = SafeGetLineNumberForOffset(oldDocument, oldMarker.Offset);
			int endLine = SafeGetLineNumberForOffset(oldDocument, oldMarker.Offset + oldMarker.Length);
			oldDocument.MarkerStrategy.RemoveMarker(oldMarker);
			oldDocument.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, startLine, endLine));
			oldDocument.CommitUpdate();
		}
		oldDocument = null;
		oldMarker = null;
	}

	private static int SafeGetLineNumberForOffset(IDocument document, int offset)
	{
		if (offset <= 0)
		{
			return 0;
		}
		if (offset >= document.TextLength)
		{
			return document.TotalNumberOfLines;
		}
		return document.GetLineNumberForOffset(offset);
	}
}
