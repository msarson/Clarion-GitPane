using ICSharpCode.Core;

namespace Clarion.Core.Redirection;

internal class StartupCode : AbstractCommand
{
	public override void Run()
	{
		OpenFileDialog.InitialiseRedirection();
	}
}
