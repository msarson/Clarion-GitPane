using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Commands;

public class SideBarMoveTabDown : AbstractMenuCommand
{
	public override void Run()
	{
		SharpDevelopSideBar sharpDevelopSideBar = (SharpDevelopSideBar)Owner;
		int tabIndexAt = sharpDevelopSideBar.GetTabIndexAt(sharpDevelopSideBar.SideBarMousePosition.X, sharpDevelopSideBar.SideBarMousePosition.Y);
		if (tabIndexAt >= 0 && tabIndexAt < sharpDevelopSideBar.Tabs.Count - 1)
		{
			SideTab value = sharpDevelopSideBar.Tabs[tabIndexAt];
			sharpDevelopSideBar.Tabs[tabIndexAt] = sharpDevelopSideBar.Tabs[tabIndexAt + 1];
			sharpDevelopSideBar.Tabs[tabIndexAt + 1] = value;
			sharpDevelopSideBar.Refresh();
		}
	}
}
