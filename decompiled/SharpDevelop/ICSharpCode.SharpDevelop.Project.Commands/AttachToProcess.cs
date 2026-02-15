using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AttachToProcess : AbstractMenuCommand
{
	public override void Run()
	{
		DebuggerService.CurrentDebugger.Attach(-1);
	}
}
