namespace SearchAndReplace;

public class ForwardTextIteratorBuilder : ITextIteratorBuilder
{
	public ITextIterator BuildTextIterator(ProvidedDocumentInformation info)
	{
		return new ForwardTextIterator(info);
	}
}
