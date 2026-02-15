using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class StopDebuggingCommand : AbstractMenuCommand
{
	public override void Run()
	{
		DebuggerService.CurrentDebugger.Stop();
	}
}
