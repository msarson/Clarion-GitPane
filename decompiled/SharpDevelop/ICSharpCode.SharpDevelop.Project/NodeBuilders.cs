using System;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

public static class NodeBuilders
{
	public static TreeNode AddProjectNode(TreeNode motherNode, IProject project)
	{
		IProjectNodeBuilder projectNodeBuilder = null;
		foreach (IProjectNodeBuilder item in AddInTree.BuildItems("/SharpDevelop/Views/ProjectBrowser/NodeBuilders", null, throwOnNotFound: true))
		{
			if (item.CanBuildProjectTree(project))
			{
				projectNodeBuilder = item;
				break;
			}
		}
		if (projectNodeBuilder != null)
		{
			return projectNodeBuilder.AddProjectNode(motherNode, project);
		}
		throw new NotImplementedException("can't create node builder for project type " + project.Language);
	}
}
