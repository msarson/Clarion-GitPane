using System.Drawing;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Gui;

public class SharpDevelopSideTabItemFactory : ISideTabItemFactory
{
	public SideTabItem CreateSideTabItem(string name)
	{
		return new SharpDevelopSideTabItem(name);
	}

	public SideTabItem CreateSideTabItem(string name, object tag)
	{
		return new SharpDevelopSideTabItem(name, tag);
	}

	public SideTabItem CreateSideTabItem(string name, object tag, Bitmap bitmap)
	{
		return new SharpDevelopSideTabItem(name, tag, bitmap);
	}
}
