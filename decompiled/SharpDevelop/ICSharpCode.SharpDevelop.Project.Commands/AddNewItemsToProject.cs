using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddNewItemsToProject : AbstractMenuCommand
{
	private FileProjectItem CreateNewFile(DirectoryNode upper, string fileName)
	{
		upper.Expanding();
		FileNode fileNode = new FileNode(fileName, FileNodeStatus.InProject);
		fileNode.AddTo(upper);
		fileNode.EnsureVisible();
		return IncludeFileInProject.IncludeFileNode(fileNode);
	}

	public override void Run()
	{
		TreeNode treeNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		DirectoryNode directoryNode = null;
		while (treeNode != null && directoryNode == null)
		{
			directoryNode = treeNode as DirectoryNode;
			treeNode = treeNode.Parent;
		}
		if (directoryNode == null)
		{
			return;
		}
		directoryNode.Expand();
		directoryNode.Expanding();
		using NewFileDialog newFileDialog = new NewFileDialog(directoryNode.Directory);
		if (newFileDialog.ShowDialog(WorkbenchSingleton.MainForm) != DialogResult.OK)
		{
			return;
		}
		bool flag = false;
		foreach (KeyValuePair<string, FileDescriptionTemplate> createdFile in newFileDialog.CreatedFiles)
		{
			FileProjectItem projectItemProperties = CreateNewFile(directoryNode, createdFile.Key);
			if (createdFile.Value.SetProjectItemProperties(projectItemProperties))
			{
				flag = true;
			}
		}
		if (flag)
		{
			directoryNode.Project.Save();
			directoryNode.RecreateSubNodes();
		}
	}
}
