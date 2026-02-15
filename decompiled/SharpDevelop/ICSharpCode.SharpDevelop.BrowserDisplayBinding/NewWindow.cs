using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class NewWindow : AbstractCommand
{
	public override void Run()
	{
		WorkbenchSingleton.Workbench.ShowView(new BrowserPane());
	}
}
