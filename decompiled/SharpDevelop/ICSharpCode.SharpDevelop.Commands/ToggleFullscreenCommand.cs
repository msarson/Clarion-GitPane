using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class ToggleFullscreenCommand : AbstractMenuCommand
{
	public override void Run()
	{
		((DefaultWorkbench)WorkbenchSingleton.Workbench).FullScreen = !((DefaultWorkbench)WorkbenchSingleton.Workbench).FullScreen;
	}
}
