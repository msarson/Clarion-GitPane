using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class EnableDisableAll : AbstractMenuCommand
{
	public override void Run()
	{
		((BookmarkPadBase)Owner).EnableDisableAll();
	}
}
