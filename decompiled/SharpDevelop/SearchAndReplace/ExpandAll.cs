using ICSharpCode.Core;

namespace SearchAndReplace;

public class ExpandAll : AbstractMenuCommand
{
	public override void Run()
	{
		SearchResultPanel.Instance.ExpandAll();
	}
}
