using ICSharpCode.Core;

namespace SoftVelocity.CWPInvoke;

public class CWDialogServiceStartCommand : AbstractMenuCommand
{
	public override void Run()
	{
		CWDialogService.Start();
	}
}
