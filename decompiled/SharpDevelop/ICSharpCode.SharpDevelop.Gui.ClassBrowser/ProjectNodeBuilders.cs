using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public static class ProjectNodeBuilders
{
	public static TreeNode AddProjectNode(ExtTreeView classBrowser, IProject project)
	{
		IProjectNodeBuilder projectNodeBuilder = null;
		foreach (IProjectNodeBuilder item in AddInTree.BuildItems("/SharpDevelop/Views/ClassBrowser/ProjectNodeBuilders", null, throwOnNotFound: true))
		{
			if (item.CanBuildProjectTree(project))
			{
				projectNodeBuilder = item;
				break;
			}
		}
		if (projectNodeBuilder != null)
		{
			return projectNodeBuilder.AddProjectNode(classBrowser, project);
		}
		throw new NotImplementedException("can't create node builder for project type " + project.Language);
	}
}
