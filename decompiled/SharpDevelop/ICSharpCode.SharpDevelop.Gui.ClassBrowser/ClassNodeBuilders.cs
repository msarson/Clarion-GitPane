using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public static class ClassNodeBuilders
{
	public static TreeNode AddClassNode(ExtTreeView classBrowser, IProject project, IClass c)
	{
		IClassNodeBuilder classNodeBuilder = null;
		foreach (IClassNodeBuilder item in AddInTree.BuildItems("/SharpDevelop/Views/ClassBrowser/ClassNodeBuilders", null, throwOnNotFound: true))
		{
			if (item.CanBuildClassTree(c))
			{
				classNodeBuilder = item;
				break;
			}
		}
		if (classNodeBuilder != null)
		{
			return classNodeBuilder.AddClassNode(classBrowser, project, c);
		}
		throw new NotImplementedException("Can't create node builder for class " + c.Name);
	}
}
