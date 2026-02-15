using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public abstract class AbstractProjectNode : ExtTreeNode
{
	private IProject project;

	public IProject Project => project;

	protected AbstractProjectNode()
	{
		project = null;
	}

	public AbstractProjectNode(IProject project)
	{
		this.project = project;
	}

	public abstract void UpdateParseInformation(ICompilationUnit oldUnit, ICompilationUnit unit);

	public abstract TreeNode GetNodeByPath(string directory, bool create);

	public abstract TreeNode ExpandNodeByPath(string directory, bool create);
}
