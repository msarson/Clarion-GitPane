using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.BrowserDisplayBinding;

namespace ICSharpCode.SharpDevelop.Gui.StartPage;

public class StartpageBrowserPane : BrowserPane
{
	private System.Windows.Forms.HelpProvider helpProvider;

	private bool refreshing;

	internal static bool ColorSchemeChanged;

	private bool _NeedRefresh;

	private bool NeedRefresh
	{
		get
		{
			if (!_NeedRefresh)
			{
				return ColorSchemeChanged;
			}
			return true;
		}
		set
		{
			_NeedRefresh = value;
		}
	}

	public StartpageBrowserPane()
		: base(showNavigation: false)
	{
		Initialize();
	}

	public StartpageBrowserPane(Uri uri)
		: base(uri, showNavigation: false)
	{
		Initialize();
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += OnWorkbench_ActiveWorkbenchWindowChanged;
	}

	private void OnWorkbench_ActiveWorkbenchWindowChanged(object sender, EventArgs e)
	{
		RefreshListOfRecent();
	}

	private void RefreshListOfRecent()
	{
		if (!refreshing && NeedRefresh)
		{
			ColorSchemeChanged = false;
			StartPageThemeService.SetColorTable();
			NeedRefresh = false;
			refreshing = true;
			string text = base.HtmlViewPane.Url.Host;
			base.HtmlViewPane.Navigate("startpage://");
			if (string.IsNullOrEmpty(text))
			{
				text = RecentOpen.defaultTypeProjects;
			}
			base.HtmlViewPane.Navigate("startpage://" + text + "/");
			refreshing = false;
		}
	}

	private void Initialize()
	{
		base.HtmlViewPane.WebBrowser.IsWebBrowserContextMenuEnabled = false;
		helpProvider = new System.Windows.Forms.HelpProvider();
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		FileInfo fileInfo = new FileInfo(entryAssembly.Location);
		string helpNamespace = Path.Combine(fileInfo.DirectoryName, "ClarionHelp.chm");
		helpProvider.HelpNamespace = helpNamespace;
		helpProvider.SetHelpKeyword(base.HtmlViewPane.WebBrowser, "StartPage.htm");
		helpProvider.SetHelpNavigator(base.HtmlViewPane.WebBrowser, HelpNavigator.Topic);
		helpProvider.SetShowHelp(base.HtmlViewPane.WebBrowser, value: true);
		helpProvider.SetHelpKeyword(base.HtmlViewPane, "StartPage.htm");
		helpProvider.SetHelpNavigator(base.HtmlViewPane, HelpNavigator.Topic);
		helpProvider.SetShowHelp(WorkbenchSingleton.helpHost, value: true);
		FileService.RecentOpen.RecentChanged += RecentChanged;
	}

	public override void Dispose()
	{
		if (WorkbenchSingleton.Workbench != null)
		{
			WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged -= OnWorkbench_ActiveWorkbenchWindowChanged;
		}
		FileService.RecentOpen.RecentChanged -= RecentChanged;
		helpProvider.Dispose();
		base.Dispose();
	}

	private void RecentChanged(object sender, RecentOpenEventArgs e)
	{
		NeedRefresh = true;
	}
}
