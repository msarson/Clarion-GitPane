using System;
using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Refactoring;

public sealed class TextEditorDocument : ICSharpCode.SharpDevelop.Dom.Refactoring.IDocument
{
	private sealed class TextEditorDocumentLine : IDocumentLine
	{
		private ICSharpCode.TextEditor.Document.IDocument doc;

		private LineSegment line;

		public int Offset => line.Offset;

		public int Length => line.Length;

		public string Text => doc.GetText(line.Offset, line.Length);

		public TextEditorDocumentLine(ICSharpCode.TextEditor.Document.IDocument doc, LineSegment line)
		{
			this.doc = doc;
			this.line = line;
		}
	}

	private ICSharpCode.TextEditor.Document.IDocument doc;

	public int TextLength => doc.TextLength;

	public int TotalNumberOfLines => doc.TotalNumberOfLines;

	public TextEditorDocument(ICSharpCode.TextEditor.Document.IDocument doc)
	{
		if (doc == null)
		{
			throw new ArgumentNullException("doc");
		}
		this.doc = doc;
	}

	public IDocumentLine GetLine(int lineNumber)
	{
		return new TextEditorDocumentLine(doc, doc.GetLineSegment(lineNumber - 1));
	}

	public int PositionToOffset(int line, int column)
	{
		return doc.PositionToOffset(new TextLocation(column - 1, line - 1));
	}

	public void Insert(int offset, string text)
	{
		doc.Insert(offset, text);
	}

	public void Remove(int offset, int length)
	{
		doc.Remove(offset, length);
	}

	public char GetCharAt(int offset)
	{
		return doc.GetCharAt(offset);
	}

	public void StartUndoableAction()
	{
		doc.UndoStack.StartUndoGroup();
	}

	public void EndUndoableAction()
	{
		doc.UndoStack.EndUndoGroup();
	}

	public void UpdateView()
	{
		doc.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
		doc.CommitUpdate();
	}
}
