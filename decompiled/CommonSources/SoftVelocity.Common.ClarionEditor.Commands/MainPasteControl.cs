using SoftVelocity.ClarionNet;
using SoftVelocity.ClarionNet.ReportItems;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class MainPasteControl : AbstractClarionReportCommand
{
	public override void Run()
	{
		if (base.View.BaseReportDesignerControl.SelectedObject is ReportItem)
		{
			base.View.ReportDesignerControl.PasteControl(isMousePoint: false);
		}
		else if (base.View.BaseReportDesignerControl.SelectedObject is ReportSection)
		{
			base.View.ReportDesignerControl.PasteControl(isMousePoint: true);
		}
	}
}
