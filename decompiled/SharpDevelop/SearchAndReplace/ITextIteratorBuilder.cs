namespace SearchAndReplace;

public interface ITextIteratorBuilder
{
	ITextIterator BuildTextIterator(ProvidedDocumentInformation info);
}
