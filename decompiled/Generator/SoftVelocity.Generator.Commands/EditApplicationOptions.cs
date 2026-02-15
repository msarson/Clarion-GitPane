using ICSharpCode.Core;

namespace SoftVelocity.Generator.Commands;

internal class EditApplicationOptions : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public override void Run()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			ApplicationService.EditGeneratorOptions();
		}
	}
}
