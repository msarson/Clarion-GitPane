using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public interface ISearchStrategy
{
	bool CompilePattern(IProgressNotificationTaskInstance monitor);

	SearchResult FindNext(ITextIterator textIterator);

	SearchResult FindNext(ITextIterator textIterator, int offset, int length);
}
