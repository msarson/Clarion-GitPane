using SoftVelocity.DataDictionary;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class AddWindowColumn : AddControlColumn
{
	public override void Run()
	{
		base.Run();
	}

	protected override bool CreateFromDDField(DDField selectedField)
	{
		if (base.View != null && base.View.WindowDesignerControl != null)
		{
			return base.View.WindowDesignerControl.CreateFromDDField(selectedField, IsMultiple);
		}
		return false;
	}
}
