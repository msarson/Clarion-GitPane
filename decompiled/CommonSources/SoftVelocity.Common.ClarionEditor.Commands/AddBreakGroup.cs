namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddBreakGroup : AbstractClarionReportCommand
{
	public override bool IsEnabled => true;

	public override void Run()
	{
		base.View.ReportDesignerControl.AddNewBreakGroupFromMenu(IsCreateSections: true);
		base.View.SetDirty(dirty: true);
	}
}
