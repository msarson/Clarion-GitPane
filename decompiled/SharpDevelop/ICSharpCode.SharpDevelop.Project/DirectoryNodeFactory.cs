using System.IO;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Project;

public static class DirectoryNodeFactory
{
	public static DirectoryNode CreateDirectoryNode(TreeNode parent, IProject project, string directory)
	{
		DirectoryNode directoryNode = new DirectoryNode(directory);
		if (!string.IsNullOrEmpty(project.AppDesignerFolder) && directory == Path.Combine(project.Directory, project.AppDesignerFolder))
		{
			directoryNode.SpecialFolder = SpecialFolder.AppDesigner;
		}
		else if (DirectoryNode.IsWebReferencesFolder(project, directory))
		{
			directoryNode = new WebReferencesFolderNode(directory);
		}
		else if (parent != null && parent is WebReferencesFolderNode)
		{
			directoryNode = new WebReferenceNode(directory);
		}
		return directoryNode;
	}

	public static DirectoryNode CreateDirectoryNode(ProjectItem item, FileNodeStatus status)
	{
		DirectoryNode directoryNode;
		if (item is WebReferencesProjectItem)
		{
			directoryNode = new WebReferencesFolderNode((WebReferencesProjectItem)item);
			directoryNode.FileNodeStatus = status;
		}
		else
		{
			directoryNode = new DirectoryNode(item.FileName.Trim('\\', '/'), status);
			directoryNode.ProjectItem = item;
		}
		return directoryNode;
	}
}
