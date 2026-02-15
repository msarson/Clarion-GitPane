using ICSharpCode.Core;

namespace SearchAndReplace;

public class FindNext : AbstractMenuCommand
{
	public override void Run()
	{
		if (SearchOptions.CurrentFindPattern.Length > 0)
		{
			SearchReplaceManager.FindNext(null);
			return;
		}
		Find find = new Find();
		find.Run();
	}
}
