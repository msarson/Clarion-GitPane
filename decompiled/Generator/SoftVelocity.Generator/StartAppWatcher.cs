using ICSharpCode.Core;

namespace SoftVelocity.Generator;

internal class StartAppWatcher : AbstractCommand
{
	public override void Run()
	{
		AppWatcher.Instance.Startup();
	}
}
