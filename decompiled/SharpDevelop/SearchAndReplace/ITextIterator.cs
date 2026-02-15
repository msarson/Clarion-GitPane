using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public interface ITextIterator
{
	ITextBufferStrategy TextBuffer { get; }

	IDocument Document { get; }

	char Current { get; }

	int Position { get; set; }

	void ResetCaret();

	char GetCharRelative(int offset);

	bool MoveAhead(int numChars);

	void Reset();

	void InformReplace(int offset, int length, int newLength);
}
