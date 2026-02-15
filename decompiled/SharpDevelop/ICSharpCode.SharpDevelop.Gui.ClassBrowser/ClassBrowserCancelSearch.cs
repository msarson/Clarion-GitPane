using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ClassBrowserCancelSearch : AbstractMenuCommand
{
	public override bool IsEnabled => ClassBrowserPad.Instance.IsInSearchMode;

	public override void Run()
	{
		ClassBrowserPad.Instance.CancelSearch();
	}
}
