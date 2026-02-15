using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Commands;

public class SideBarDeleteTabHeader : AbstractMenuCommand
{
	public override void Run()
	{
		SharpDevelopSideBar sharpDevelopSideBar = (SharpDevelopSideBar)Owner;
		SideTab tabAt = sharpDevelopSideBar.GetTabAt(sharpDevelopSideBar.SideBarMousePosition.X, sharpDevelopSideBar.SideBarMousePosition.Y);
		if (MessageBox.Show(StringParser.Parse(ResourceService.GetString("SideBarComponent.ContextMenu.DeleteTabHeaderQuestion"), new string[1, 2] { { "TabHeader", tabAt.DisplayName } }), ResourceService.GetString("Global.QuestionText"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
		{
			sharpDevelopSideBar.DeleteSideTab(tabAt);
			sharpDevelopSideBar.Refresh();
		}
	}
}
