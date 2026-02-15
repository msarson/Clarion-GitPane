using ICSharpCode.Core;

namespace SearchAndReplace;

public class BackToNormalView : AbstractMenuCommand
{
	public override void Run()
	{
		SearchResultPanel.Instance.RemoveSpecialPanel();
	}
}
