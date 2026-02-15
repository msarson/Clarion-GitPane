using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class CreateMissingCommand : AbstractMenuCommand
{
	public override void Run()
	{
		TreeNode selectedNode = ProjectBrowserPad.Instance.ProjectBrowserControl.SelectedNode;
		DirectoryNode directoryNode = selectedNode as DirectoryNode;
		Directory.CreateDirectory(directoryNode.Directory);
		IncludeFileInProject.IncludeDirectoryNode(directoryNode, includeSubNodes: false);
	}
}
