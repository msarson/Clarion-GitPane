using SoftVelocity.DataDictionary;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddReportColumn : AddControlColumn
{
	public override void Run()
	{
		base.Run();
	}

	protected override bool CreateFromDDField(DDField selectedField)
	{
		if (base.View != null && base.View.ReportDesignerControl != null)
		{
			return base.View.BaseReportDesignerControl.CreateFromDDField(selectedField, IsMultiple);
		}
		return false;
	}
}
