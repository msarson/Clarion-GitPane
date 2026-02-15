namespace SoftVelocity.Common.ClarionEditor.Commands;

public class ClaRedoCommand : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.BaseReportDesignerControl != null && base.View.ReportDesignerControl != null)
			{
				return base.View.ReportDesignerControl.IsRedo();
			}
			return false;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.Redo();
	}
}
