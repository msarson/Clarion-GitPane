using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class GoSearch : AbstractCommand
{
	public override void Run()
	{
		((HtmlViewPane)Owner).GoSearch();
	}
}
