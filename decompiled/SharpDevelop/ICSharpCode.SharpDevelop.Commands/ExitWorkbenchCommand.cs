using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class ExitWorkbenchCommand : AbstractMenuCommand
{
	public override void Run()
	{
		((Form)WorkbenchSingleton.Workbench).Close();
	}
}
