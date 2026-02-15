using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Commands;

public class SideBarRenameTabItem : AbstractMenuCommand
{
	public override void Run()
	{
		SharpDevelopSideBar sharpDevelopSideBar = (SharpDevelopSideBar)Owner;
		SideTabItem choosedItem = sharpDevelopSideBar.ActiveTab.ChoosedItem;
		if (choosedItem != null)
		{
			sharpDevelopSideBar.StartRenamingOf(choosedItem);
		}
	}
}
