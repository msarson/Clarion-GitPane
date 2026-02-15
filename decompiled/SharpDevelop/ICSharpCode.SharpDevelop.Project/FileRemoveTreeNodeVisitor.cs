using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class FileRemoveTreeNodeVisitor : ProjectBrowserTreeNodeVisitor
{
	private string fileName;

	public FileRemoveTreeNodeVisitor(string fileName)
	{
		this.fileName = fileName;
	}

	public override object Visit(SolutionItemNode solutionItemNode, object data)
	{
		if (FileUtility.IsBaseDirectory(fileName, solutionItemNode.FileName))
		{
			solutionItemNode.Remove();
		}
		else
		{
			solutionItemNode.AcceptChildren(this, data);
		}
		return data;
	}

	public override object Visit(ProjectNode projectNode, object data)
	{
		if (FileUtility.IsBaseDirectory(projectNode.Directory, fileName))
		{
			projectNode.AcceptChildren(this, data);
		}
		return data;
	}

	public override object Visit(DirectoryNode directoryNode, object data)
	{
		if (FileUtility.IsBaseDirectory(fileName, directoryNode.Directory))
		{
			ExtTreeNode extTreeNode = directoryNode.Parent as ExtTreeNode;
			directoryNode.Remove();
			extTreeNode?.Refresh();
		}
		else if (FileUtility.IsBaseDirectory(directoryNode.Directory, fileName))
		{
			directoryNode.AcceptChildren(this, data);
		}
		return data;
	}

	public override object Visit(FileNode fileNode, object data)
	{
		if (FileUtility.IsBaseDirectory(fileName, fileNode.FileName))
		{
			ExtTreeNode extTreeNode = fileNode.Parent as ExtTreeNode;
			fileNode.Remove();
			extTreeNode?.Refresh();
		}
		else
		{
			fileNode.AcceptChildren(this, data);
		}
		return data;
	}
}
