using ICSharpCode.Core;

namespace SearchAndReplace;

public class ClearResult : AbstractMenuCommand
{
	public override void Run()
	{
		SearchResultPanel.Instance.Clear();
	}
}
