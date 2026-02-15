using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ClassBrowserNavigateForward : AbstractMenuCommand
{
	public override bool IsEnabled => ClassBrowserPad.Instance.CanNavigateForward;

	public override void Run()
	{
		ClassBrowserPad.Instance.NavigateForward();
	}
}
