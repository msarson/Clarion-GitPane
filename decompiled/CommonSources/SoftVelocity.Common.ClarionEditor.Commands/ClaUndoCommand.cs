namespace SoftVelocity.Common.ClarionEditor.Commands;

public class ClaUndoCommand : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View != null && base.View.BaseReportDesignerControl != null && base.View.ReportDesignerControl != null)
			{
				return base.View.ReportDesignerControl.IsUndo();
			}
			return false;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.Undo();
	}
}
