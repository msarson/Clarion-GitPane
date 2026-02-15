using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class SideBarMoveActiveItemUp : AbstractMenuCommand
{
	public override void Run()
	{
		SharpDevelopSideBar sharpDevelopSideBar = (SharpDevelopSideBar)Owner;
		int num = sharpDevelopSideBar.ActiveTab.Items.IndexOf(sharpDevelopSideBar.ActiveTab.ChoosedItem);
		if (num > 0)
		{
			sharpDevelopSideBar.ActiveTab.Exchange(num - 1, num);
			sharpDevelopSideBar.Refresh();
		}
	}
}
