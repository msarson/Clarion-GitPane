using SoftVelocity.ClarionNet.ReportItems;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class PasteControl : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View.BaseReportDesignerControl.SelectedObject is ReportItem && base.View.ReportDesignerControl.IsUserControlCopy())
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.PasteControl(isMousePoint: false);
	}
}
