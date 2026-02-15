using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Commands;

public class SideBarMoveActiveTabUp : AbstractMenuCommand
{
	public override void Run()
	{
		SharpDevelopSideBar sharpDevelopSideBar = (SharpDevelopSideBar)Owner;
		int num = sharpDevelopSideBar.Tabs.IndexOf(sharpDevelopSideBar.ActiveTab);
		if (num > 0)
		{
			SideTab value = sharpDevelopSideBar.Tabs[num];
			sharpDevelopSideBar.Tabs[num] = sharpDevelopSideBar.Tabs[num - 1];
			sharpDevelopSideBar.Tabs[num - 1] = value;
			sharpDevelopSideBar.Refresh();
		}
	}
}
