using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Project;

public interface IProjectNodeBuilder
{
	bool CanBuildProjectTree(IProject project);

	TreeNode AddProjectNode(TreeNode motherNode, IProject project);
}
