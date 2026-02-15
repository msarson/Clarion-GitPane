namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AlignBottom : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsAlignBottom())
			{
				return false;
			}
			return true;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.AlignBottom();
	}
}
