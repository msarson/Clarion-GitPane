namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddDetail : AbstractClarionReportCommand
{
	public override void Run()
	{
		base.View.ReportDesignerControl.AddDetailFromMenu(null);
		base.View.SetDirty(dirty: true);
	}
}
