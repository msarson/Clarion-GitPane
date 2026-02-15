using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class GoBack : AbstractCommand
{
	public override void Run()
	{
		((HtmlViewPane)Owner).WebBrowser.GoBack();
	}
}
