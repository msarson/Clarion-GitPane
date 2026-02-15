namespace SoftVelocity.Common.ClarionEditor.Commands;

public class SpreadHorz : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsSpreadHorz())
			{
				return false;
			}
			return true;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.SpreadHorz();
	}
}
