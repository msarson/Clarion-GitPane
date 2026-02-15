namespace SoftVelocity.Common.ClarionEditor.Commands;

public class PasteSectionControl : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsUserControlCopy())
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.PasteControl(isMousePoint: true);
	}
}
