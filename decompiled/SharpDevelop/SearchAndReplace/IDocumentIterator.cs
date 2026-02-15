namespace SearchAndReplace;

public interface IDocumentIterator
{
	ProvidedDocumentInformation Current { get; }

	string CurrentFileName { get; }

	bool MoveForward();

	bool MoveBackward();

	void Reset();
}
