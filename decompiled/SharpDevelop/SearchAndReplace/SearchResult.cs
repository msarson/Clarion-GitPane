using System;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class SearchResult
{
	private ProvidedDocumentInformation providedDocumentInformation;

	private int offset;

	private int length;

	public string FileName => providedDocumentInformation.FileName;

	public ProvidedDocumentInformation ProvidedDocumentInformation
	{
		set
		{
			providedDocumentInformation = value;
		}
	}

	public int Offset => offset;

	public int Length => length;

	public virtual string DisplayText => null;

	public virtual string TransformReplacePattern(string pattern)
	{
		return pattern;
	}

	public IDocument CreateDocument()
	{
		return providedDocumentInformation.CreateDocument();
	}

	public SearchResult(int offset, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		this.offset = offset;
		this.length = length;
	}

	public virtual TextLocation GetStartPosition(IDocument document)
	{
		return document.OffsetToPosition(Offset);
	}

	public virtual TextLocation GetEndPosition(IDocument document)
	{
		return document.OffsetToPosition(Offset + Length);
	}

	public override string ToString()
	{
		return $"[SearchResult: FileName={FileName}, Offset={Offset}, Length={Length}]";
	}
}
