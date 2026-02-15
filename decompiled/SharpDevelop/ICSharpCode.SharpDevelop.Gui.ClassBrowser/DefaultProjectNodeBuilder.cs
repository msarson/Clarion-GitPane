using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class DefaultProjectNodeBuilder : IProjectNodeBuilder
{
	public bool CanBuildProjectTree(IProject project)
	{
		return true;
	}

	public TreeNode AddProjectNode(ExtTreeView classBrowser, IProject project)
	{
		ProjectNode projectNode = new ProjectNode(project);
		projectNode.AddTo(classBrowser);
		classBrowser.Sort();
		return projectNode;
	}
}
