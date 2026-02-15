using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public interface ISchemeExtension
{
	void InterceptNavigate(HtmlViewPane pane, WebBrowserNavigatingEventArgs e);

	void DocumentCompleted(HtmlViewPane pane, WebBrowserDocumentCompletedEventArgs e);

	void GoHome(HtmlViewPane pane);

	void GoSearch(HtmlViewPane pane);
}
