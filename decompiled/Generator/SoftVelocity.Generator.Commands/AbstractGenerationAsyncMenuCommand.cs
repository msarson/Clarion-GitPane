using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.Commands;

internal abstract class AbstractGenerationAsyncMenuCommand : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			return ApplicationService.ApplicationsList.Count > 0;
		}
		set
		{
		}
	}

	public override void Run()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(new Action(ExecuteApplicationService));
		}
	}

	public abstract void ExecuteApplicationService();
}
