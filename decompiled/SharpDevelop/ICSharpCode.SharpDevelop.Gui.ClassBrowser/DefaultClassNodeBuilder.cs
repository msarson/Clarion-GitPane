using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class DefaultClassNodeBuilder : IClassNodeBuilder
{
	public bool CanBuildClassTree(IClass c)
	{
		return true;
	}

	public TreeNode AddClassNode(ExtTreeView classBrowser, IProject project, IClass c)
	{
		ClassNode classNode = new ClassNode(project, c);
		classNode.AddTo(classBrowser);
		return classNode;
	}
}
