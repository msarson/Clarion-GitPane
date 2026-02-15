using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class AboutSharpDevelop : AbstractMenuCommand
{
	public override void Run()
	{
		using CommonAboutDialog commonAboutDialog = new CommonAboutDialog();
		commonAboutDialog.Owner = (Form)WorkbenchSingleton.Workbench;
		commonAboutDialog.ShowDialog(WorkbenchSingleton.MainForm);
	}
}
