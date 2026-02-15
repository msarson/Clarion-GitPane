using ICSharpCode.Core;

namespace SearchAndReplace;

public class CollapseAll : AbstractMenuCommand
{
	public override void Run()
	{
		SearchResultPanel.Instance.CollapseAll();
	}
}
