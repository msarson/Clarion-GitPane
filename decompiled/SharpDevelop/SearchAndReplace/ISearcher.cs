using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public interface ISearcher
{
	void RunAll(SearchType action, IProgressNotificationTaskInstance monitor);

	void FindNext();

	void Replace();

	void Init();
}
