namespace SoftVelocity.Common.ClarionEditor.Commands;

public class CollapseAll : AbstractClarionReportCommand
{
	public override bool IsEnabled => base.View.IsCollapseAll();

	public override void Run()
	{
		base.View.CollapseAll();
	}
}
