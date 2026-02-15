using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddNewSolutionFolderToSolution : AbstractMenuCommand
{
	public override void Run()
	{
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		ISolutionFolderNode solutionFolderNode = selectedNode as ISolutionFolderNode;
		if (selectedNode != null)
		{
			SolutionFolder folder = solutionFolderNode.Solution.CreateFolder(ResourceService.GetString("ProjectComponent.NewFolderString"));
			solutionFolderNode.Container.AddFolder(folder);
			solutionFolderNode.Solution.Save();
			SolutionFolderNode solutionFolderNode2 = new SolutionFolderNode(solutionFolderNode.Solution, folder);
			solutionFolderNode2.AddTo(selectedNode);
			ProjectBrowserPad.Instance.StartLabelEdit(solutionFolderNode2);
		}
	}
}
