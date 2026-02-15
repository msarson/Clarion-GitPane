namespace SoftVelocity.Common.ClarionEditor.Commands;

public class CollapseSection : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null)
			{
				return base.View.ReportDesignerControl.IsCollapseSection();
			}
			return false;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.CollapseSection();
	}
}
