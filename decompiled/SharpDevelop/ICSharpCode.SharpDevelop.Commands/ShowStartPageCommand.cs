using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.BrowserDisplayBinding;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.StartPage;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Commands;

public class ShowStartPageCommand : AbstractMenuCommand
{
	private static bool isFirstStartPage = true;

	public override void Run()
	{
		DoRun();
	}

	internal static void DoRun()
	{
		if (isFirstStartPage)
		{
			isFirstStartPage = false;
			ProjectService.SolutionLoaded += delegate
			{
				if (PropertyService.Get("SharpDevelop.CloseStartPageOnSolutionOpening", defaultValue: true))
				{
					IViewContent[] array = WorkbenchSingleton.Workbench.ViewContentCollection.ToArray();
					foreach (IViewContent viewContent in array)
					{
						if (viewContent is BrowserPane browserPane2 && browserPane2.Url.Scheme == "startpage")
						{
							StatusBarService.ClearMessage();
							browserPane2.WorkbenchWindow.CloseWindow(force: true);
						}
					}
				}
			};
		}
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			if (item is BrowserPane browserPane && browserPane.Url.Scheme == "startpage")
			{
				StatusBarService.ClearMessage();
				item.WorkbenchWindow.SelectWindow();
				return;
			}
		}
		StatusBarService.SetMessage("");
		WorkbenchSingleton.Workbench.ShowView(new StartpageBrowserPane(new Uri("startpage://" + RecentOpen.defaultTypeProjects + "/")));
	}
}
