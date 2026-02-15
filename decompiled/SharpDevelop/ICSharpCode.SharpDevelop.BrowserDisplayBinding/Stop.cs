using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class Stop : AbstractCommand
{
	public override void Run()
	{
		((HtmlViewPane)Owner).WebBrowser.Stop();
	}
}
