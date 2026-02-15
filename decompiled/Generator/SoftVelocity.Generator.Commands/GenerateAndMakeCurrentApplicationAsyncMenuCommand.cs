using ICSharpCode.Core;

namespace SoftVelocity.Generator.Commands;

internal class GenerateAndMakeCurrentApplicationAsyncMenuCommand : GenerateCurrentApplicationAsyncMenuCommand
{
	public override void ExecuteApplicationService()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			ApplicationService.GenerateAndMakeApplication(app);
		}
	}
}
