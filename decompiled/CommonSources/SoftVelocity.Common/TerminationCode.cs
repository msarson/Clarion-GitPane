using Clarion.ASL;
using ICSharpCode.Core;
using SoftVelocity.CWPInvoke;

namespace SoftVelocity.Common;

internal class TerminationCode : AbstractCommand
{
	public override void Run()
	{
		if (StartupCode.Called)
		{
			CWDialogService.Stop();
			Commands.FinishASL((IASLInit)null);
			StartupCode.Called = false;
		}
	}
}
