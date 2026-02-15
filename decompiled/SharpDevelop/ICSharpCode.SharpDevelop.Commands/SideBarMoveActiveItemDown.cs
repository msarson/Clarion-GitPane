using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class SideBarMoveActiveItemDown : AbstractMenuCommand
{
	public override void Run()
	{
		SharpDevelopSideBar sharpDevelopSideBar = (SharpDevelopSideBar)Owner;
		int num = sharpDevelopSideBar.ActiveTab.Items.IndexOf(sharpDevelopSideBar.ActiveTab.ChoosedItem);
		if (num >= 0 && num < sharpDevelopSideBar.ActiveTab.Items.Count - 1)
		{
			sharpDevelopSideBar.ActiveTab.Exchange(num, num + 1);
			sharpDevelopSideBar.Refresh();
		}
	}
}
