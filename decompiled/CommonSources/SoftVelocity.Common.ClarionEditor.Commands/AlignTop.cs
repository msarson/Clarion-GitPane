namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AlignTop : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsAlignTop())
			{
				return false;
			}
			return true;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.AlignTop();
	}
}
