using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddExistingItemToSolution : AbstractMenuCommand
{
	public override void Run()
	{
		if (ProjectBrowserPad.Instance.ProjectBrowserControl.RootNode == null)
		{
			return;
		}
		if (ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode == null)
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.SelectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.RootNode;
		}
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		ISolutionFolderNode solutionFolderNode = selectedNode as ISolutionFolderNode;
		if (selectedNode == null)
		{
			return;
		}
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		openFileDialog.AddExtension = true;
		openFileDialog.Filter = StringParser.Parse("${res:SharpDevelop.FileFilter.AllFiles}|*.*");
		openFileDialog.Multiselect = true;
		openFileDialog.CheckFileExists = true;
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			string[] fileNames = openFileDialog.FileNames;
			foreach (string fileName in fileNames)
			{
				solutionFolderNode.AddItem(fileName);
			}
			ProjectService.SaveSolution();
		}
	}
}
