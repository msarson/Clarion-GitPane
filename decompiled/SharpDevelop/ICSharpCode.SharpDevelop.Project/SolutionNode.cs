using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionNode : AbstractProjectBrowserTreeNode, ISolutionFolderNode
{
	private Solution solution;

	public ISolutionFolder Folder => solution;

	public override Solution Solution => solution;

	public ISolutionFolderContainer Container => solution;

	public override bool EnablePaste => SolutionFolderNode.DoEnablePaste(this);

	public SolutionNode(Solution solution)
	{
		sortOrder = -1;
		this.solution = solution;
		UpdateText();
		autoClearNodes = false;
		canLabelEdit = true;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/SolutionNode";
		SetIcon("ProjectBrowser.Solution");
		base.Tag = solution;
	}

	public override void BeforeLabelEdit()
	{
		base.Text = solution.Name;
	}

	public override void AfterLabelEdit(string newName)
	{
		try
		{
			if (newName != null && !(solution.Name == newName) && FileService.CheckFileName(newName))
			{
				string text = Path.Combine(solution.Directory, newName + ".sln");
				if (FileService.RenameFile(solution.FileName, text, isDirectory: false))
				{
					solution.FileName = text;
					solution.Name = newName;
				}
			}
		}
		finally
		{
			UpdateText();
		}
	}

	private void UpdateText()
	{
		base.Text = ResourceService.GetString("ICSharpCode.SharpDevelop.Commands.ProjectBrowser.SolutionNodeText") + " " + solution.Name;
	}

	public void AddItem(string fileName)
	{
		string text = ResourceService.GetString("ICSharpCode.SharpDevelop.Commands.ProjectBrowser.SolutionItemsNodeText");
		SolutionFolderNode solutionFolderNode = null;
		foreach (TreeNode node in base.Nodes)
		{
			solutionFolderNode = node as SolutionFolderNode;
			if (solutionFolderNode == null || !(solutionFolderNode.Folder.Name == text))
			{
				solutionFolderNode = null;
				continue;
			}
			break;
		}
		if (solutionFolderNode == null)
		{
			SolutionFolder folder = solution.CreateFolder(text);
			solution.AddFolder(folder);
			solution.Save();
			solutionFolderNode = new SolutionFolderNode(solution, folder);
			solutionFolderNode.AddTo(this);
		}
		solutionFolderNode.AddItem(fileName);
	}

	public override DragDropEffects GetDragDropEffect(IDataObject dataObject, DragDropEffects proposedEffect)
	{
		if (dataObject.GetDataPresent(typeof(SolutionFolderNode)))
		{
			SolutionFolderNode solutionFolderNode = (SolutionFolderNode)dataObject.GetData(typeof(SolutionFolderNode));
			if (solutionFolderNode.Folder.Parent != solution)
			{
				return DragDropEffects.All;
			}
		}
		if (dataObject.GetDataPresent(typeof(ProjectNode)))
		{
			ProjectNode projectNode = (ProjectNode)dataObject.GetData(typeof(ProjectNode));
			if (projectNode.Parent != this)
			{
				return DragDropEffects.Move;
			}
		}
		return DragDropEffects.None;
	}

	public override void DoDragDrop(IDataObject dataObject, DragDropEffects effect)
	{
		AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode = null;
		if (dataObject.GetDataPresent(typeof(SolutionFolderNode)))
		{
			SolutionFolderNode solutionFolderNode = (SolutionFolderNode)dataObject.GetData(typeof(SolutionFolderNode));
			abstractProjectBrowserTreeNode = solutionFolderNode.Parent as AbstractProjectBrowserTreeNode;
			solutionFolderNode.Remove();
			solutionFolderNode.AddTo(this);
			solution.AddFolder(solutionFolderNode.Folder);
		}
		if (dataObject.GetDataPresent(typeof(ProjectNode)))
		{
			ProjectNode projectNode = (ProjectNode)dataObject.GetData(typeof(ProjectNode));
			abstractProjectBrowserTreeNode = projectNode.Parent as AbstractProjectBrowserTreeNode;
			projectNode.Remove();
			projectNode.AddTo(this);
			projectNode.EnsureVisible();
			solution.AddFolder(projectNode.Project);
		}
		abstractProjectBrowserTreeNode?.Refresh();
		solution.Save();
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
	}

	public override void Paste()
	{
		SolutionFolderNode.DoPaste(this);
	}
}
