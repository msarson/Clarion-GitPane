using System;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class SelectClassBrowserFilter : AbstractMenuCommand
{
	private ToolBarDropDownButton dropDownButton;

	public override void Run()
	{
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		dropDownButton = (ToolBarDropDownButton)Owner;
		ToolStripItem[] array = (ToolStripItem[])AddInTree.GetTreeNode("/SharpDevelop/Pads/ClassBrowser/Toolbar/SelectFilter").BuildChildItems(this).ToArray(typeof(ToolStripItem));
		ToolStripItem[] array2 = array;
		foreach (ToolStripItem toolStripItem in array2)
		{
			if (toolStripItem is IStatusUpdate)
			{
				((IStatusUpdate)toolStripItem).UpdateStatus();
			}
		}
		dropDownButton.DropDownItems.AddRange(array);
	}
}
