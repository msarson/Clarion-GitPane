using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.BrowserDisplayBinding;
using ICSharpCode.SharpDevelop.Commands;

namespace ICSharpCode.SharpDevelop.Gui.StartPage;

public class StartPageScheme : DefaultSchemeExtension
{
	private static StartPageScheme _Instance = new StartPageScheme();

	private StartPageHtmlGenerator page;

	internal static StartPageScheme Instance => _Instance;

	public override void InterceptNavigate(HtmlViewPane pane, WebBrowserNavigatingEventArgs e)
	{
		e.Cancel = true;
		string host = e.Url.Host;
		if (e.Url.AbsolutePath == "/" || page == null)
		{
			page = new StartPageHtmlGenerator();
			page.Title = StringParser.Parse("${res:StartPage.StartPageContentName}");
			page.TopMenuSelectedItem = host;
			pane.WebBrowser.DocumentText = page.Render(host);
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (int.TryParse(e.Url.LocalPath.Trim('/'), out var result))
		{
			if (result >= 10000)
			{
				result -= 10000;
				if (result >= 10000)
				{
					result -= 10000;
					flag2 = true;
				}
				else
				{
					flag = true;
				}
			}
			RecentOpen.RecentOpenDescription recentFileDescription = page.GetRecentFileDescription(host, result);
			if (recentFileDescription != null)
			{
				if (flag)
				{
					string directoryName = Path.GetDirectoryName(Path.GetFullPath(recentFileDescription.FileName));
					if (directoryName != string.Empty)
					{
						Process.Start(directoryName + "\\.");
						return;
					}
				}
				else
				{
					if (flag2)
					{
						page.RemoveRecentFileDescription(host, result);
						pane.Navigate("startpage://" + host + "/");
						return;
					}
					if (page.HasRecentEvent("OpenRecent", host) && page.CreateRecentEvent("OpenRecent", host) is AbstractRecentOpenCommand abstractRecentOpenCommand)
					{
						abstractRecentOpenCommand.FileDescription = recentFileDescription;
						abstractRecentOpenCommand.Run();
						return;
					}
				}
			}
		}
		page.TopMenuSelectedItem = host;
		pane.WebBrowser.DocumentText = page.Render(host);
	}

	public override void DocumentCompleted(HtmlViewPane pane, WebBrowserDocumentCompletedEventArgs e)
	{
		if (page == null)
		{
			return;
		}
		HtmlElement elementById = pane.WebBrowser.Document.GetElementById("Open" + page.GetOriginalSectionName(page.TopMenuSelectedItem));
		if (elementById != null)
		{
			LoggingService.Debug("Attached event handler to open " + page.TopMenuSelectedItem + " button");
			elementById.Click += delegate
			{
				page.CreateRecentEvent("Open", page.TopMenuSelectedItem).Run();
			};
		}
		elementById = pane.WebBrowser.Document.GetElementById("New" + page.GetOriginalSectionName(page.TopMenuSelectedItem));
		if (elementById != null)
		{
			LoggingService.Debug("Attached event handler to new " + page.TopMenuSelectedItem + " button");
			elementById.Click += delegate
			{
				page.CreateRecentEvent("New", page.TopMenuSelectedItem).Run();
			};
		}
		elementById = pane.WebBrowser.Document.GetElementById("DeleteMissingCheckbox");
		if (elementById != null)
		{
			elementById.Click += delegate
			{
				bool flag = PropertyService.Get("RemoveMissingRecents", defaultValue: true);
				PropertyService.Set("RemoveMissingRecents", !flag);
			};
		}
	}

	public override void GoHome(HtmlViewPane pane)
	{
		pane.Navigate("startpage://" + RecentOpen.defaultTypeProjects + "/");
	}
}
