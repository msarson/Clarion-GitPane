namespace SoftVelocity.Common.ClarionEditor.Commands;

public class SurroundingBreakSection : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsSurroundingBreak())
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.SurroundingBreak();
	}
}
