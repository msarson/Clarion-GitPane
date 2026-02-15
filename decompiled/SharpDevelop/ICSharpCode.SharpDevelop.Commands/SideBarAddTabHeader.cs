using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Commands;

public class SideBarAddTabHeader : AbstractMenuCommand
{
	public override void Run()
	{
		SharpDevelopSideBar sharpDevelopSideBar = (SharpDevelopSideBar)Owner;
		SideTab sideTab = new SideTab(sharpDevelopSideBar, "New Tab");
		sharpDevelopSideBar.Tabs.Add(sideTab);
		sharpDevelopSideBar.StartRenamingOf(sideTab);
		sharpDevelopSideBar.DoAddTab = true;
		sharpDevelopSideBar.Refresh();
	}
}
