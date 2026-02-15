using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class ProvidedDocumentInformation
{
	private IDocument document;

	private ITextBufferStrategy textBuffer;

	private string fileName;

	private int currentOffset;

	private TextAreaControl textAreaControl;

	private int endOffset;

	public ITextBufferStrategy TextBuffer
	{
		get
		{
			return textBuffer;
		}
		set
		{
			textBuffer = value;
		}
	}

	public string FileName => fileName;

	public IDocument Document => document;

	public int CurrentOffset
	{
		get
		{
			if (textAreaControl != null)
			{
				return textAreaControl.Caret.Offset;
			}
			return currentOffset;
		}
		set
		{
			if (textAreaControl != null)
			{
				textAreaControl.Caret.Position = document.OffsetToPosition(value + 1);
			}
			else
			{
				currentOffset = value;
			}
		}
	}

	public int EndOffset => endOffset;

	public void Replace(int offset, int length, string pattern)
	{
		if (document != null)
		{
			document.Replace(offset, length, pattern);
		}
		else
		{
			textBuffer.Replace(offset, length, pattern);
		}
		if (offset <= CurrentOffset)
		{
			CurrentOffset = CurrentOffset - length + pattern.Length;
		}
	}

	public IDocument CreateDocument()
	{
		if (document != null)
		{
			return document;
		}
		return new DocumentFactory().CreateFromTextBuffer(textBuffer);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ProvidedDocumentInformation providedDocumentInformation))
		{
			return false;
		}
		if (fileName == providedDocumentInformation.fileName)
		{
			return textAreaControl == providedDocumentInformation.textAreaControl;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return fileName.GetHashCode();
	}

	public ProvidedDocumentInformation(IDocument document, string fileName, int currentOffset)
	{
		this.document = document;
		textBuffer = document.TextBufferStrategy;
		this.fileName = fileName;
		endOffset = (this.currentOffset = currentOffset);
	}

	public ProvidedDocumentInformation(IDocument document, string fileName, TextAreaControl textAreaControl)
	{
		this.document = document;
		textBuffer = document.TextBufferStrategy;
		this.fileName = fileName;
		this.textAreaControl = textAreaControl;
		endOffset = CurrentOffset;
	}

	public ProvidedDocumentInformation(ITextBufferStrategy textBuffer, string fileName, int currentOffset)
	{
		this.textBuffer = textBuffer;
		this.fileName = fileName;
		endOffset = (this.currentOffset = currentOffset);
	}
}
