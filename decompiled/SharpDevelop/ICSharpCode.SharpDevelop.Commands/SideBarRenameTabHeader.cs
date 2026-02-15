using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class SideBarRenameTabHeader : AbstractMenuCommand
{
	public override void Run()
	{
		SharpDevelopSideBar sharpDevelopSideBar = (SharpDevelopSideBar)Owner;
		sharpDevelopSideBar.StartRenamingOf(sharpDevelopSideBar.GetTabAt(sharpDevelopSideBar.SideBarMousePosition.X, sharpDevelopSideBar.SideBarMousePosition.Y));
	}
}
