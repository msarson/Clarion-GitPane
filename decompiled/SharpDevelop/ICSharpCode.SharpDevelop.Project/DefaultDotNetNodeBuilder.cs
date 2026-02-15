using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

public class DefaultDotNetNodeBuilder : IProjectNodeBuilder
{
	public bool CanBuildProjectTree(IProject project)
	{
		return true;
	}

	public TreeNode AddProjectNode(TreeNode motherNode, IProject project)
	{
		ProjectNode projectNode = new ProjectNode(project);
		projectNode.AddTo(motherNode);
		if (project is MissingProject)
		{
			CustomNode customNode = new CustomNode();
			customNode.SetIcon("Icons.16x16.Warning");
			customNode.Text = ResourceService.GetString("ICSharpCode.SharpDevelop.Commands.ProjectBrowser.ProjectFileNotFound");
			customNode.AddTo(projectNode);
		}
		else if (project is UnknownProject)
		{
			string extension = Path.GetExtension(project.FileName);
			if (".proj".Equals(extension, StringComparison.OrdinalIgnoreCase) || ".build".Equals(extension, StringComparison.OrdinalIgnoreCase))
			{
				string openedImage = (projectNode.ClosedImage = "Icons.16x16.XMLFileIcon");
				projectNode.OpenedImage = openedImage;
				projectNode.Nodes.Clear();
			}
			else
			{
				CustomNode customNode2 = new CustomNode();
				customNode2.SetIcon("Icons.16x16.Warning");
				customNode2.Text = StringParser.Parse(((UnknownProject)project).WarningText);
				customNode2.AddTo(projectNode);
			}
		}
		else
		{
			new ReferenceFolder(project).AddTo(projectNode);
		}
		return projectNode;
	}
}
