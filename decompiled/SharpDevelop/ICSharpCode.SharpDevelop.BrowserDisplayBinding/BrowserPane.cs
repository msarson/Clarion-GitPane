using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class BrowserPane : AbstractViewContent
{
	private HtmlViewPane htmlViewPane;

	public HtmlViewPane HtmlViewPane => htmlViewPane;

	public override Control Control => htmlViewPane;

	public override bool IsDirty
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public override bool IsViewOnly => true;

	public Uri Url => htmlViewPane.Url;

	protected BrowserPane(bool showNavigation)
	{
		htmlViewPane = new HtmlViewPane(showNavigation);
		htmlViewPane.WebBrowser.DocumentTitleChanged += TitleChange;
		htmlViewPane.Closed += PaneClosed;
		TitleChange(null, null);
	}

	protected BrowserPane(Uri uri, bool showNavigation)
		: this(showNavigation)
	{
		htmlViewPane.Navigate(uri);
	}

	public BrowserPane(Uri uri)
		: this(uri, showNavigation: true)
	{
	}

	public BrowserPane()
		: this(showNavigation: true)
	{
	}

	public override void Dispose()
	{
		base.Dispose();
		htmlViewPane.Dispose();
	}

	public override void Load(string url)
	{
		htmlViewPane.Navigate(url);
	}

	public override void Save(string url)
	{
		Load(url);
	}

	private void PaneClosed(object sender, EventArgs e)
	{
		WorkbenchWindow.CloseWindow(force: true);
		StatusBarService.ClearMessage();
	}

	private void TitleChange(object sender, EventArgs e)
	{
		string text = htmlViewPane.WebBrowser.DocumentTitle;
		if (text != null)
		{
			text = text.Trim();
		}
		if (text == null || text.Length == 0)
		{
			TitleName = ResourceService.GetString("ICSharpCode.SharpDevelop.BrowserDisplayBinding.Browser");
		}
		else
		{
			TitleName = text;
		}
	}

	public override INavigationPoint BuildNavPoint()
	{
		return null;
	}
}
