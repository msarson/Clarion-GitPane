using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ExcludeFileFromProject : AbstractMenuCommand
{
	public static void ExcludeFileNode(FileNode fileNode)
	{
		List<FileNode> list = new List<FileNode>();
		foreach (TreeNode node in fileNode.Nodes)
		{
			if (node is FileNode)
			{
				list.Add((FileNode)node);
			}
		}
		list.ForEach(ExcludeFileNode);
		bool isLink = fileNode.IsLink;
		if (fileNode.ProjectItem != null)
		{
			ProjectService.RemoveProjectItem(fileNode.Project, fileNode.ProjectItem);
		}
		if (isLink)
		{
			fileNode.Remove();
			return;
		}
		fileNode.ProjectItem = null;
		fileNode.FileNodeStatus = FileNodeStatus.None;
		if (fileNode.Parent is ExtTreeNode)
		{
			((ExtTreeNode)fileNode.Parent).UpdateVisibility();
		}
	}

	private static void ExcludeDirectoryNode(DirectoryNode directoryNode)
	{
		if (directoryNode.ProjectItem != null)
		{
			ProjectService.RemoveProjectItem(directoryNode.Project, directoryNode.ProjectItem);
			directoryNode.ProjectItem = null;
		}
		directoryNode.FileNodeStatus = FileNodeStatus.None;
		if (directoryNode.Parent is ExtTreeNode)
		{
			((ExtTreeNode)directoryNode.Parent).UpdateVisibility();
		}
	}

	public override void Run()
	{
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.SelectedNode;
		if (selectedNode == null)
		{
			return;
		}
		if (selectedNode is FileNode)
		{
			ExcludeFileNode((FileNode)selectedNode);
		}
		else if (selectedNode is DirectoryNode)
		{
			selectedNode.Expanding();
			Stack<TreeNode> stack = new Stack<TreeNode>();
			stack.Push(selectedNode);
			while (stack.Count > 0)
			{
				TreeNode treeNode = stack.Pop();
				if (treeNode is FileNode)
				{
					ExcludeFileNode((FileNode)treeNode);
				}
				else if (treeNode is DirectoryNode)
				{
					ExcludeDirectoryNode((DirectoryNode)treeNode);
				}
				foreach (TreeNode node in treeNode.Nodes)
				{
					if (node is ExtTreeNode)
					{
						((ExtTreeNode)node).Expanding();
					}
					stack.Push(node);
				}
			}
		}
		ProjectService.SaveSolution();
		((AbstractProjectBrowserTreeNode)selectedNode.Parent).Refresh();
	}
}
