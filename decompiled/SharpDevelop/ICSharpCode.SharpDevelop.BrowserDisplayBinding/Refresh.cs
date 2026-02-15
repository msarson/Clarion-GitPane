using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class Refresh : AbstractCommand
{
	public override void Run()
	{
		if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
		{
			((HtmlViewPane)Owner).WebBrowser.Refresh(WebBrowserRefreshOption.Completely);
		}
		else
		{
			((HtmlViewPane)Owner).WebBrowser.Refresh();
		}
	}
}
