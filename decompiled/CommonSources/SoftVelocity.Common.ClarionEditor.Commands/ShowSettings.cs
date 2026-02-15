namespace SoftVelocity.Common.ClarionEditor.Commands;

public class ShowSettings : AbstractClarionReportCommand
{
	public override void Run()
	{
		base.View.ShowReportSettings(isReport: false);
	}
}
