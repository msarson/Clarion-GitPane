namespace SoftVelocity.Common.ClarionEditor.Commands;

public class MakeSameSize : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsMakeSameSize())
			{
				return false;
			}
			return true;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.MakeSameSize();
	}
}
