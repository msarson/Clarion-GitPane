using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class GoHome : AbstractCommand
{
	public override void Run()
	{
		((HtmlViewPane)Owner).GoHome();
	}
}
