using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.StartPage;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class HtmlViewPane : UserControl
{
	public const string DefaultHomepage = "http://www.softvelocity.com/";

	public const string DefaultSearchUrl = "http://www.google.com/";

	private ExtendedWebBrowser webBrowser;

	private ToolStrip toolStrip;

	private static ArrayList descriptors;

	private Control urlBox;

	private string dummyUrl;

	public ExtendedWebBrowser WebBrowser => webBrowser;

	public Uri Url
	{
		get
		{
			if (webBrowser.Url == null)
			{
				return new Uri("about:blank");
			}
			if (dummyUrl != null && webBrowser.Url.ToString() == "about:blank")
			{
				return new Uri(dummyUrl);
			}
			return webBrowser.Url;
		}
	}

	public event EventHandler Closed;

	public void Close()
	{
		StatusBarService.ClearMessage();
		if (this.Closed != null)
		{
			this.Closed(this, EventArgs.Empty);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			webBrowser.Dispose();
		}
	}

	public HtmlViewPane(bool showNavigation)
	{
		Dock = DockStyle.Fill;
		base.Size = new Size(500, 500);
		webBrowser = new ExtendedWebBrowser();
		webBrowser.Dock = DockStyle.Fill;
		webBrowser.Navigating += WebBrowserNavigating;
		webBrowser.NewWindowExtended += NewWindow;
		webBrowser.Navigated += WebBrowserNavigated;
		webBrowser.StatusTextChanged += WebBrowserStatusTextChanged;
		webBrowser.DocumentCompleted += WebBrowserDocumentCompleted;
		base.Controls.Add(webBrowser);
		if (showNavigation)
		{
			toolStrip = ToolbarService.CreateToolStrip(this, "/SharpDevelop/ViewContent/Browser/Toolbar");
			toolStrip.GripStyle = ToolStripGripStyle.Hidden;
			base.Controls.Add(toolStrip);
		}
	}

	private void NewWindow(object sender, NewWindowExtendedEventArgs e)
	{
		e.Cancel = true;
		WorkbenchSingleton.Workbench.ShowView(new BrowserPane(e.Url));
	}

	private void WebBrowserStatusTextChanged(object sender, EventArgs e)
	{
		if (webBrowser.StatusText != null && !webBrowser.StatusText.StartsWith("startpage:"))
		{
			IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
			if (activeWorkbenchWindow != null && activeWorkbenchWindow.ActiveViewContent is BrowserPane browserPane && browserPane.HtmlViewPane == this)
			{
				StatusBarService.SetMessage(webBrowser.StatusText);
			}
		}
	}

	public static ISchemeExtension GetScheme(string name)
	{
		if (descriptors == null)
		{
			descriptors = AddInTree.BuildItems("/SharpDevelop/Views/Browser/SchemeExtensions", null, throwOnNotFound: false);
		}
		if (name.Equals("startpage", StringComparison.OrdinalIgnoreCase))
		{
			return StartPageScheme.Instance;
		}
		foreach (SchemeExtensionDescriptor descriptor in descriptors)
		{
			if (string.Equals(name, descriptor.SchemeName, StringComparison.OrdinalIgnoreCase))
			{
				return descriptor.Extension;
			}
		}
		return null;
	}

	private void WebBrowserNavigating(object sender, WebBrowserNavigatingEventArgs e)
	{
		try
		{
			ISchemeExtension scheme = GetScheme(e.Url.Scheme);
			if (scheme == null)
			{
				return;
			}
			scheme.InterceptNavigate(this, e);
			StatusBarService.ClearMessage();
			if (e.TargetFrameName.Length == 0)
			{
				if (e.Cancel)
				{
					dummyUrl = e.Url.ToString();
				}
				else if (e.Url.ToString() != "about:blank")
				{
					dummyUrl = null;
				}
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}

	private void WebBrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
	{
		try
		{
			if (dummyUrl != null && e.Url.ToString() == "about:blank")
			{
				e = new WebBrowserDocumentCompletedEventArgs(new Uri(dummyUrl));
			}
			GetScheme(e.Url.Scheme)?.DocumentCompleted(this, e);
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}

	public void Navigate(string url)
	{
		webBrowser.Navigate(new Uri(url));
	}

	public void Navigate(Uri url)
	{
		webBrowser.Navigate(url);
		StatusBarService.ClearMessage();
	}

	public void GoHome()
	{
		ISchemeExtension scheme = GetScheme(Url.Scheme);
		if (scheme != null)
		{
			scheme.GoHome(this);
		}
		else
		{
			Navigate("http://www.softvelocity.com/");
		}
	}

	public void GoSearch()
	{
		ISchemeExtension scheme = GetScheme(Url.Scheme);
		if (scheme != null)
		{
			scheme.GoSearch(this);
		}
		else
		{
			Navigate("http://www.google.com/");
		}
	}

	public void SetUrlComboBox(ComboBox comboBox)
	{
		SetUrlBox(comboBox);
		comboBox.DropDownStyle = ComboBoxStyle.DropDown;
		comboBox.Items.Clear();
		comboBox.Items.AddRange(PropertyService.Get("Browser.URLBoxHistory", new string[0]));
		comboBox.AutoCompleteMode = AutoCompleteMode.Suggest;
		comboBox.AutoCompleteSource = AutoCompleteSource.HistoryList;
	}

	public void SetUrlBox(Control urlBox)
	{
		this.urlBox = urlBox;
		urlBox.KeyUp += UrlBoxKeyUp;
	}

	private void UrlBoxKeyUp(object sender, KeyEventArgs e)
	{
		Control ctl = (Control)sender;
		if (e.KeyData == Keys.Return)
		{
			e.Handled = true;
			UrlBoxNavigate(ctl);
		}
	}

	private void UrlBoxNavigate(Control ctl)
	{
		string text = ctl.Text.Trim();
		if (text.IndexOf(':') < 0)
		{
			text = "http://" + text;
		}
		Navigate(text);
		if (!(ctl is ComboBox comboBox))
		{
			return;
		}
		comboBox.Items.Remove(text);
		comboBox.Items.Insert(0, text);
		string[] array = PropertyService.Get("Browser.URLBoxHistory", new string[0]);
		int num = Array.IndexOf(array, text);
		if (num < 0 && array.Length >= 20)
		{
			num = array.Length - 1;
		}
		if (num < 0)
		{
			string[] array2 = new string[array.Length + 1];
			array.CopyTo(array2, 1);
			array = array2;
		}
		else
		{
			for (int num2 = num; num2 > 0; num2--)
			{
				array[num2] = array[num2 - 1];
			}
		}
		array[0] = text;
		PropertyService.Set("Browser.URLBoxHistory", array);
	}

	private void WebBrowserNavigated(object sender, WebBrowserNavigatedEventArgs e)
	{
		if (urlBox != null)
		{
			string text = webBrowser.Url.ToString();
			if (dummyUrl != null && text == "about:blank")
			{
				urlBox.Text = dummyUrl;
			}
			else
			{
				urlBox.Text = text;
			}
		}
		if (toolStrip == null)
		{
			return;
		}
		foreach (object item in toolStrip.Items)
		{
			if (item is IStatusUpdate statusUpdate)
			{
				statusUpdate.UpdateStatus();
			}
		}
	}
}
