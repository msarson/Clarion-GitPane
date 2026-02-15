using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddNewFolderToProject : AbstractMenuCommand
{
	private string GenerateValidDirectoryName(string inDirectory)
	{
		string text = Path.Combine(inDirectory, ResourceService.GetString("ProjectComponent.NewFolderString"));
		if (Directory.Exists(text))
		{
			int i;
			for (i = 1; Directory.Exists(text + i); i++)
			{
			}
			return text + i;
		}
		return text;
	}

	private DirectoryNode CreateNewDirectory(DirectoryNode upper, string directoryName)
	{
		upper.Expanding();
		Directory.CreateDirectory(directoryName);
		DirectoryNode directoryNode = new DirectoryNode(directoryName, FileNodeStatus.InProject);
		directoryNode.AddTo(upper);
		IncludeFileInProject.IncludeDirectoryNode(directoryNode, includeSubNodes: false);
		return directoryNode;
	}

	public override void Run()
	{
		TreeNode selectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		DirectoryNode directoryNode = selectedNode as DirectoryNode;
		if (directoryNode == null)
		{
			directoryNode = selectedNode.Parent as DirectoryNode;
		}
		if (directoryNode != null)
		{
			directoryNode.Expanding();
			string directoryName = GenerateValidDirectoryName(directoryNode.Directory);
			DirectoryNode node = CreateNewDirectory(directoryNode, directoryName);
			ProjectBrowserPad.Instance.StartLabelEdit(node);
		}
	}
}
