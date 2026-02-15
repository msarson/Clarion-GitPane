namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddPageHeader : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsPageHeader())
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.AddNewPageHeader())
		{
			base.View.SetDirty(dirty: true);
		}
	}
}
