namespace SoftVelocity.Common.ClarionEditor.Commands;

public class DeleteSection : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsReportSelected())
			{
				return false;
			}
			return true;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.DeleteSection();
	}
}
