using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project.Dialogs;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class CreateNewSolution : AbstractMenuCommand
{
	public override void Run()
	{
		using NewProjectDialog newProjectDialog = new NewProjectDialog(createNewSolution: true);
		newProjectDialog.Owner = (Form)WorkbenchSingleton.Workbench;
		newProjectDialog.ShowDialog(WorkbenchSingleton.MainForm);
	}
}
