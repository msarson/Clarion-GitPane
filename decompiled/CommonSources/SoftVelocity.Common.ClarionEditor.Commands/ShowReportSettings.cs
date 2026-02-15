namespace SoftVelocity.Common.ClarionEditor.Commands;

public class ShowReportSettings : AbstractClarionReportCommand
{
	public override void Run()
	{
		base.View.ShowReportSettings(isReport: true);
	}
}
