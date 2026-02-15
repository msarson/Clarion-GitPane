using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project.Dialogs;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddNewProjectToSolution : AbstractMenuCommand
{
	public override void Run()
	{
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		ISolutionFolderNode solutionFolderNode = selectedNode as ISolutionFolderNode;
		if (solutionFolderNode == null)
		{
			return;
		}
		if (solutionFolderNode is SolutionFolderNode && solutionFolderNode.Folder.IdGuid == "{2150E333-8FDC-42A3-9474-1A3956D46DE8}" && ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.Nodes.Count > 0)
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.SelectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.Nodes[0];
			selectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
			solutionFolderNode = selectedNode as ISolutionFolderNode;
		}
		using NewProjectDialog newProjectDialog = new NewProjectDialog(createNewSolution: false);
		if (PropertyService.Get("SharpDevelop.UseSolutionFolderInsteadDefault", defaultValue: true))
		{
			newProjectDialog.DefaultProjectPath = solutionFolderNode.Solution.Directory;
		}
		if (newProjectDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			if (newProjectDialog.NewProjectLocation.Length == 0)
			{
				MessageService.ShowError("No project has been created, there is nothing to add.");
				return;
			}
			AddExitingProjectToSolution.AddProject(solutionFolderNode, newProjectDialog.NewProjectLocation);
			ProjectService.SaveSolution();
		}
	}
}
