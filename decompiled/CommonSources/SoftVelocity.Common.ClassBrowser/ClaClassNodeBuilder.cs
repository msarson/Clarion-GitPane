using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.ClassBrowser;

public class ClaClassNodeBuilder : IClassNodeBuilder
{
	public bool CanBuildClassTree(IClass c)
	{
		if (c is ClaClass)
		{
			return true;
		}
		return false;
	}

	public TreeNode AddClassNode(ExtTreeView classBrowser, IProject project, IClass c)
	{
		if (!(project is CommonClarionProject))
		{
			return null;
		}
		ClaClassNode claClassNode = new ClaClassNode(project, c);
		((ExtTreeNode)claClassNode).AddTo((TreeView)(object)classBrowser);
		return (TreeNode)(object)claClassNode;
	}
}
