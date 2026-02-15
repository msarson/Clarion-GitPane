using SoftVelocity.ClarionNet;
using SoftVelocity.ClarionNet.ReportItems;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class MainDeleteControl : AbstractClarionReportCommand
{
	public override void Run()
	{
		if (base.View.BaseReportDesignerControl != null)
		{
			if (base.View.BaseReportDesignerControl.SelectedObject is ReportItem)
			{
				base.View.ReportDesignerControl.DeleteControl();
			}
			else if (base.View.BaseReportDesignerControl.SelectedObject is ReportSection)
			{
				base.View.ReportDesignerControl.DeleteSection();
			}
		}
	}
}
