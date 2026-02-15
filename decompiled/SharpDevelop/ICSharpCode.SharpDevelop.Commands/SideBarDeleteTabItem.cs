using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Commands;

public class SideBarDeleteTabItem : AbstractMenuCommand
{
	public override void Run()
	{
		SharpDevelopSideBar sharpDevelopSideBar = (SharpDevelopSideBar)Owner;
		SideTabItem choosedItem = sharpDevelopSideBar.ActiveTab.ChoosedItem;
		if (choosedItem != null && MessageBox.Show(StringParser.Parse(ResourceService.GetString("SideBarComponent.ContextMenu.DeleteTabItemQuestion"), new string[1, 2] { { "TabItem", choosedItem.Name } }), ResourceService.GetString("Global.QuestionText"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
		{
			sharpDevelopSideBar.ActiveTab.Items.Remove(choosedItem);
			sharpDevelopSideBar.Refresh();
		}
	}
}
