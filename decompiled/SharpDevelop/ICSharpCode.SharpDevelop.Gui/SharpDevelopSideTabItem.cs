using System.Drawing;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Gui;

public class SharpDevelopSideTabItem : SideTabItem
{
	public SharpDevelopSideTabItem(string name)
		: base(name)
	{
		base.Icon = ResourceService.GetBitmap("Icons.16x16.SideBarDocument");
	}

	public SharpDevelopSideTabItem(string name, object tag)
		: base(name, tag)
	{
		base.Icon = ResourceService.GetBitmap("Icons.16x16.SideBarDocument");
	}

	public SharpDevelopSideTabItem(string name, object tag, Bitmap icon)
		: base(name, tag, icon)
	{
	}
}
