using ICSharpCode.Core;

namespace SoftVelocity.Generator.Commands;

public abstract class AbstractEditTemplateRegistryMenuCommand : AbstractMenuCommand
{
	protected abstract bool ForWindows { get; }

	public override bool IsEnabled
	{
		get
		{
			return !ApplicationService.AreApplicationOnEdit;
		}
		set
		{
		}
	}

	public override void Run()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			ApplicationService.EditTemplateRegistry(ForWindows, null);
		}
	}
}
