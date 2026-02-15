using SoftVelocity.ClarionNet.ReportItems;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class DeleteControl : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View.BaseReportDesignerControl.SelectedObject is ReportItem)
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.DeleteControl();
	}
}
