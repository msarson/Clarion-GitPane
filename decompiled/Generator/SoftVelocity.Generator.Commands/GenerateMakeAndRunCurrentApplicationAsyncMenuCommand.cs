using ICSharpCode.Core;

namespace SoftVelocity.Generator.Commands;

internal class GenerateMakeAndRunCurrentApplicationAsyncMenuCommand : GenerateCurrentApplicationAsyncMenuCommand
{
	public override void ExecuteApplicationService()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			ApplicationService.MakeAndRunApplication(app);
		}
	}
}
