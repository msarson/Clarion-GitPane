using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ClassBrowserCommitSearch : AbstractMenuCommand
{
	public override void Run()
	{
		ClassBrowserPad.Instance.StartSearch();
	}
}
