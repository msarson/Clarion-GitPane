using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class DetachDebugger : AbstractMenuCommand
{
	public override void Run()
	{
		DebuggerService.CurrentDebugger.Detach();
	}
}
