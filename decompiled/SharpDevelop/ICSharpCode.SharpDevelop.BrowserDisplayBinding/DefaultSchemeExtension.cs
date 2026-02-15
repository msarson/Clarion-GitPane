using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class DefaultSchemeExtension : ISchemeExtension
{
	public virtual void InterceptNavigate(HtmlViewPane pane, WebBrowserNavigatingEventArgs e)
	{
	}

	public virtual void DocumentCompleted(HtmlViewPane pane, WebBrowserDocumentCompletedEventArgs e)
	{
	}

	public virtual void GoHome(HtmlViewPane pane)
	{
		pane.Navigate("http://www.softvelocity.com/");
	}

	public virtual void GoSearch(HtmlViewPane pane)
	{
		pane.Navigate("http://www.google.com/");
	}
}
