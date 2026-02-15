using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class BreakDebuggingCommand : AbstractMenuCommand
{
	public override void Run()
	{
		DebuggerService.CurrentDebugger.Break();
	}
}
