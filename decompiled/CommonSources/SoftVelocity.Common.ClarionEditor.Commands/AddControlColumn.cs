using SoftVelocity.ClarionNet.Designer;
using SoftVelocity.DataDictionary;
using SoftVelocity.Generator;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public abstract class AddControlColumn : AbstractClarionGeneratorCommand
{
	protected virtual bool IsMultiple => false;

	protected abstract bool CreateFromDDField(DDField selectedField);

	public override void Run()
	{
		AddColumn();
	}

	protected bool AddColumn()
	{
		IFormatter formatterRequester = base.View.FormatterRequester;
		DDField dDField = SelectFieldDialogActions.FileSchemaSelectDialog(formatterRequester.Schema, SelectType.Field);
		if (dDField == null)
		{
			return false;
		}
		return CreateFromDDField(dDField);
	}
}
