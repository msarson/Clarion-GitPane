using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class OpenContentsMenuBuilder : ISubmenuBuilder
{
	private class MyMenuItem : MenuCheckBox
	{
		private IViewContent content;

		public MyMenuItem(IViewContent content)
			: base(StringParser.Parse(content.TitleName))
		{
			this.content = content;
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			base.Checked = true;
			content.WorkbenchWindow.SelectWindow();
		}
	}

	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		int count = WorkbenchSingleton.Workbench.ViewContentCollection.Count;
		if (count == 0)
		{
			return new ToolStripItem[0];
		}
		ToolStripItem[] array = new ToolStripItem[count + 1];
		array[0] = new MenuSeparator(null, null);
		for (int i = 0; i < count; i++)
		{
			IViewContent viewContent = WorkbenchSingleton.Workbench.ViewContentCollection[i];
			if (viewContent.WorkbenchWindow != null)
			{
				MenuCheckBox menuCheckBox = new MyMenuItem(viewContent);
				menuCheckBox.Tag = viewContent.WorkbenchWindow;
				menuCheckBox.Checked = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow == viewContent.WorkbenchWindow;
				menuCheckBox.Description = "Activate this window ";
				array[i + 1] = menuCheckBox;
			}
		}
		return array;
	}
}
