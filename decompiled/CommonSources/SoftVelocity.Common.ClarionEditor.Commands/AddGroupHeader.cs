namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddGroupHeader : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsGroupHeader())
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		if (base.View.ReportDesignerControl.AddGroupHeader())
		{
			base.View.SetDirty(dirty: true);
		}
	}
}
