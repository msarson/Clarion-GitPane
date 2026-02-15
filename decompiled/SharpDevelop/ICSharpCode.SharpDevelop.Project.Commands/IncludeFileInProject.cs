using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class IncludeFileInProject : AbstractMenuCommand
{
	public static FileProjectItem IncludeFileNode(FileNode fileNode)
	{
		if (fileNode.Parent is FileNode && ((FileNode)fileNode.Parent).FileNodeStatus != FileNodeStatus.InProject)
		{
			IncludeFileNode((FileNode)fileNode.Parent);
		}
		if (fileNode.Parent is DirectoryNode && !(fileNode.Parent is ProjectNode) && ((DirectoryNode)fileNode.Parent).FileNodeStatus != FileNodeStatus.InProject)
		{
			IncludeDirectoryNode((DirectoryNode)fileNode.Parent, includeSubNodes: false);
		}
		ItemType defaultItemType = fileNode.Project.GetDefaultItemType(fileNode.FileName);
		FileProjectItem fileProjectItem = new FileProjectItem(fileNode.Project, defaultItemType);
		fileProjectItem.Include = FileUtility.GetRelativePath(fileNode.Project.Directory, fileNode.FileName);
		ProjectService.AddProjectItem(fileNode.Project, fileProjectItem);
		fileNode.ProjectItem = fileProjectItem;
		fileNode.FileNodeStatus = FileNodeStatus.InProject;
		if (fileNode.Parent is ExtTreeNode)
		{
			((ExtTreeNode)fileNode.Parent).UpdateVisibility();
		}
		fileNode.Project.Save();
		return fileProjectItem;
	}

	public static void IncludeDirectoryNode(DirectoryNode directoryNode, bool includeSubNodes)
	{
		if (directoryNode.Parent is DirectoryNode && !(directoryNode.Parent is ProjectNode) && ((DirectoryNode)directoryNode.Parent).FileNodeStatus != FileNodeStatus.InProject)
		{
			IncludeDirectoryNode((DirectoryNode)directoryNode.Parent, includeSubNodes: false);
		}
		FileProjectItem fileProjectItem = new FileProjectItem(directoryNode.Project, ItemType.Folder, FileUtility.GetRelativePath(directoryNode.Project.Directory, directoryNode.Directory));
		ProjectService.AddProjectItem(directoryNode.Project, fileProjectItem);
		directoryNode.ProjectItem = fileProjectItem;
		directoryNode.FileNodeStatus = FileNodeStatus.InProject;
		if (includeSubNodes)
		{
			foreach (TreeNode node in directoryNode.Nodes)
			{
				if (node is ExtTreeNode)
				{
					((ExtTreeNode)node).Expanding();
				}
				if (node is FileNode)
				{
					IncludeFileNode((FileNode)node);
				}
				else if (node is DirectoryNode)
				{
					IncludeDirectoryNode((DirectoryNode)node, includeSubNodes);
				}
			}
		}
		directoryNode.Project.Save();
	}

	public override void Run()
	{
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.SelectedNode;
		if (selectedNode != null)
		{
			selectedNode.Expanding();
			if (selectedNode is FileNode)
			{
				IncludeFileNode((FileNode)selectedNode);
			}
			else if (selectedNode is DirectoryNode)
			{
				IncludeDirectoryNode((DirectoryNode)selectedNode, includeSubNodes: true);
			}
			ProjectService.SaveSolution();
			((AbstractProjectBrowserTreeNode)selectedNode.Parent).Refresh();
		}
	}
}
