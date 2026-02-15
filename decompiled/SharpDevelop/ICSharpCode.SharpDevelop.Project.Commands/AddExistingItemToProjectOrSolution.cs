using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddExistingItemToProjectOrSolution : AbstractMenuCommand
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
		AbstractMenuCommand abstractMenuCommand = null;
		TreeNode selectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		DirectoryNode directoryNode = selectedNode as DirectoryNode;
		if (directoryNode == null)
		{
			directoryNode = selectedNode.Parent as DirectoryNode;
			if (directoryNode != null)
			{
				ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.SelectedNode = directoryNode;
			}
		}
		if (directoryNode != null)
		{
			abstractMenuCommand = new AddExistingItemsToProject();
		}
		else
		{
			ISolutionFolderNode solutionFolderNode = selectedNode as ISolutionFolderNode;
			if (solutionFolderNode == null && selectedNode is SolutionItemNode && selectedNode.Parent != null)
			{
				solutionFolderNode = selectedNode.Parent as ISolutionFolderNode;
			}
			if (solutionFolderNode != null)
			{
				ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.SelectedNode = solutionFolderNode as TreeNode;
				abstractMenuCommand = new AddExistingItemToSolution();
			}
		}
		abstractMenuCommand?.Run();
	}
}
