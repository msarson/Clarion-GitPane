using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionFolderRemoveVisitor : ProjectBrowserTreeNodeVisitor
{
	private ISolutionFolder folder;

	public SolutionFolderRemoveVisitor(ISolutionFolder folder)
	{
		this.folder = folder;
	}

	public override object Visit(SolutionFolderNode solutionFolderNode, object data)
	{
		if (folder == solutionFolderNode.Folder)
		{
			ExtTreeNode extTreeNode = solutionFolderNode.Parent as ExtTreeNode;
			solutionFolderNode.Remove();
			extTreeNode?.Refresh();
		}
		else
		{
			solutionFolderNode.AcceptChildren(this, data);
		}
		return data;
	}

	public override object Visit(ProjectNode projectNode, object data)
	{
		if (folder == projectNode.Project)
		{
			ExtTreeNode extTreeNode = projectNode.Parent as ExtTreeNode;
			projectNode.Remove();
			extTreeNode?.Refresh();
		}
		return data;
	}
}
