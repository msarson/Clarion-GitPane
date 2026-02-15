using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class CloseAllWindows : AbstractMenuCommand
{
	public override void Run()
	{
		WorkbenchSingleton.Workbench.CloseAllViews();
	}
}
