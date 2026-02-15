namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddGroupFooter : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsGroupFooter())
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		if (base.View.ReportDesignerControl.AddGroupFooter())
		{
			base.View.SetDirty(dirty: true);
		}
	}
}
