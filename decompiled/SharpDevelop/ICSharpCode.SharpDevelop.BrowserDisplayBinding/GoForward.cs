using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class GoForward : AbstractCommand
{
	public override void Run()
	{
		((HtmlViewPane)Owner).WebBrowser.GoForward();
	}
}
