using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class SnapGridBuilder : ISubmenuBuilder
{
	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		if (!(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is CommonClarionDesignerView))
		{
			throw new NullReferenceException("ClaReportViewBuilder : No view available");
		}
		ToolStripMenuItem[] array = new ToolStripMenuItem[1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new ToolStripMenuItem("&Snap to Grid");
		}
		return array;
	}

	private void SnapToGridClick(object sender, EventArgs e)
	{
	}
}
