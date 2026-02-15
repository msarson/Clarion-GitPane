using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class ViewBuilder : ISubmenuBuilder
{
	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		if (!(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is CommonClarionDesignerView commonClarionDesignerView))
		{
			throw new NullReferenceException("ClaReportViewBuilder : No view available");
		}
		ToolStripMenuItem[] array = new ToolStripMenuItem[2]
		{
			new ToolStripMenuItem("Band &View"),
			null
		};
		array[0].Click += BandViewClick;
		array[0].Checked = commonClarionDesignerView.ReportDesignerControl != null && commonClarionDesignerView.ReportDesignerControl.IsBandView();
		array[1] = new ToolStripMenuItem("Page &Layout View");
		array[1].Click += PageViewClick;
		array[1].Checked = !array[0].Checked;
		return array;
	}

	private void BandViewClick(object sender, EventArgs e)
	{
		_ = (ToolStripMenuItem)sender;
		if (!(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is CommonClarionDesignerView commonClarionDesignerView))
		{
			throw new NullReferenceException("ClaReportViewBuilder : No view available");
		}
		commonClarionDesignerView.ReportDesignerControl.SetBandView(isband: true);
	}

	private void PageViewClick(object sender, EventArgs e)
	{
		_ = (ToolStripMenuItem)sender;
		if (!(WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent is CommonClarionDesignerView commonClarionDesignerView))
		{
			throw new NullReferenceException("ClaReportViewBuilder : No view available");
		}
		commonClarionDesignerView.ReportDesignerControl.SetBandView(isband: false);
	}
}
