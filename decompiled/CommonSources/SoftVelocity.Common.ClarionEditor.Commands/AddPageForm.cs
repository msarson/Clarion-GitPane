namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddPageForm : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.ReportDesignerControl != null && base.View.ReportDesignerControl.IsPageForm())
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		if (base.View.ReportDesignerControl.AddNewPageForm())
		{
			base.View.SetDirty(dirty: true);
		}
	}
}
