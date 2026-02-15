namespace SoftVelocity.Common.ClarionEditor.Commands;

public class CutControl : AbstractClarionReportCommand
{
	public override bool IsEnabled => true;

	public override void Run()
	{
		base.View.ReportDesignerControl.CutControl();
	}
}
