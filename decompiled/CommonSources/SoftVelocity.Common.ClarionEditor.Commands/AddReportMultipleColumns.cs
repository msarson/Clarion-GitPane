namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddReportMultipleColumns : AddReportColumn
{
	protected override bool IsMultiple => true;

	public override void Run()
	{
		base.Run();
	}
}
