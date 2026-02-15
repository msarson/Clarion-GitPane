namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddPageFooter : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsPageFooter())
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.AddNewPageFooter())
		{
			base.View.SetDirty(dirty: true);
		}
	}
}
