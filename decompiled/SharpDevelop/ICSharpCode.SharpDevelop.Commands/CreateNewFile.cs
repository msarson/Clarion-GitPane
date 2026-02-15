using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Commands;

public class CreateNewFile : AbstractMenuCommand
{
	public override void Run()
	{
		if (ProjectBrowserPad.Instance.CurrentProject != null)
		{
			switch (MessageService.ShowCustomDialog("${res:Dialog.NewFile.AddToProjectQuestionTitle}", "${res:Dialog.NewFile.AddToProjectQuestion}", "${res:Dialog.NewFile.AddToProjectQuestionProject}", "${res:Dialog.NewFile.AddToProjectQuestionStandalone}"))
			{
			case 0:
				ProjectBrowserPad.Instance.CurrentProject.AddNewItemsToProject();
				return;
			case -1:
				return;
			}
		}
		using NewFileDialog newFileDialog = new NewFileDialog(null);
		newFileDialog.Owner = (Form)WorkbenchSingleton.Workbench;
		newFileDialog.ShowDialog(WorkbenchSingleton.MainForm);
	}
}
