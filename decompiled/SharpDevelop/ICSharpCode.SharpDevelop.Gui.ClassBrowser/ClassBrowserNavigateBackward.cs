using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ClassBrowserNavigateBackward : AbstractMenuCommand
{
	public override bool IsEnabled => ClassBrowserPad.Instance.CanNavigateBackward;

	public override void Run()
	{
		ClassBrowserPad.Instance.NavigateBackward();
	}
}
