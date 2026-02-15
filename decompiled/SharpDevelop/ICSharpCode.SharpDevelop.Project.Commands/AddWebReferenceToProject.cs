using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddWebReferenceToProject : AbstractMenuCommand
{
	public override void Run()
	{
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.SelectedNode;
		if (selectedNode == null || selectedNode.Project == null)
		{
			return;
		}
		using AddWebReferenceDialog addWebReferenceDialog = new AddWebReferenceDialog(selectedNode.Project);
		addWebReferenceDialog.NamespacePrefix = selectedNode.Project.RootNamespace;
		if (addWebReferenceDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		addWebReferenceDialog.WebReference.Save();
		foreach (ProjectItem item in addWebReferenceDialog.WebReference.Items)
		{
			ProjectService.AddProjectItem(selectedNode.Project, item);
		}
		AddWebReferenceToProjectBrowser(selectedNode, addWebReferenceDialog.WebReference);
		ParserService.ParseFile(addWebReferenceDialog.WebReference.WebProxyFileName);
		selectedNode.Project.Save();
	}

	private void AddWebReferenceToProjectBrowser(AbstractProjectBrowserTreeNode node, WebReference webReference)
	{
		TreeNode treeNode = null;
		if (node is ProjectNode)
		{
			treeNode = AddWebReferenceToProjectNode((ProjectNode)node, webReference);
		}
		else if (node is WebReferencesFolderNode)
		{
			treeNode = node;
			WebReferenceNodeBuilder.AddWebReference((WebReferencesFolderNode)treeNode, webReference);
		}
		else if (node is ReferenceFolder && node.Parent != null && node.Parent is ProjectNode)
		{
			treeNode = AddWebReferenceToProjectNode((ProjectNode)node.Parent, webReference);
		}
		else
		{
			LoggingService.Warn("AddWebReferenceToProjectBrowser: Selected node type is not handled.");
			AddWebReferenceToProjectBrowser(node.Parent as AbstractProjectBrowserTreeNode, webReference);
		}
		if (treeNode != null)
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.TreeView.Sort();
			treeNode.Expand();
			treeNode.EnsureVisible();
		}
	}

	private TreeNode GetWebReferencesFolderNode(ProjectNode projectNode)
	{
		foreach (TreeNode node in projectNode.Nodes)
		{
			if (node is WebReferencesFolderNode result)
			{
				return result;
			}
		}
		return null;
	}

	private TreeNode AddWebReferenceToProjectNode(ProjectNode node, WebReference webReference)
	{
		TreeNode treeNode = WebReferenceNodeBuilder.AddWebReferencesFolderNode(node, webReference);
		if (treeNode == null)
		{
			treeNode = GetWebReferencesFolderNode(node);
			if (treeNode != null)
			{
				WebReferenceNodeBuilder.AddWebReference((WebReferencesFolderNode)treeNode, webReference);
			}
		}
		return treeNode;
	}
}
