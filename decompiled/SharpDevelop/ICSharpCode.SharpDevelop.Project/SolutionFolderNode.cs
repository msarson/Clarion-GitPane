using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionFolderNode : CustomFolderNode, ISolutionFolderNode
{
	private Solution solution;

	private SolutionFolder folder;

	public override Solution Solution => solution;

	public ISolutionFolder Folder => folder;

	public ISolutionFolderContainer Container => folder;

	public override bool EnableDelete => true;

	public override bool EnableCopy => false;

	public override bool EnableCut => true;

	public override bool EnablePaste => DoEnablePaste(this);

	public override DataObject DragDropDataObject => new DataObject(this);

	public SolutionFolderNode(Solution solution, SolutionFolder folder)
	{
		sortOrder = 0;
		canLabelEdit = true;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/SolutionFolderNode";
		this.solution = solution;
		this.folder = folder;
		base.Tag = folder;
		base.Text = folder.Name;
		autoClearNodes = false;
		if (!folder.IsEmpty)
		{
			base.Nodes.Add(new CustomNode());
		}
		base.OpenedImage = "ProjectBrowser.SolutionFolder.Open";
		base.ClosedImage = "ProjectBrowser.SolutionFolder.Closed";
	}

	public override void AfterLabelEdit(string newName)
	{
		if (FileService.CheckFileName(newName))
		{
			SolutionFolder solutionFolder = folder;
			string text = (folder.Name = newName);
			string text3 = (solutionFolder.Location = text);
			base.Text = text3;
			solution.Save();
		}
	}

	public void AddItem(string fileName)
	{
		string relativePath = FileUtility.GetRelativePath(solution.Directory, fileName);
		SolutionItem item = new SolutionItem(relativePath, relativePath);
		folder.SolutionItems.Items.Add(item);
		SolutionItemNode.NewNode(solution, item).AddTo(this);
	}

	protected override void Initialize()
	{
		base.Nodes.Clear();
		foreach (ISolutionFolder folder in folder.Folders)
		{
			if (folder is IProject)
			{
				NodeBuilders.AddProjectNode(this, (IProject)folder);
			}
			else if (folder is SolutionFolder)
			{
				SolutionFolderNode solutionFolderNode = new SolutionFolderNode(solution, (SolutionFolder)folder);
				solutionFolderNode.AddTo(this);
			}
			else
			{
				MessageService.ShowWarning("SolutionFolderNode.Initialize(): unknown tree object : " + folder);
			}
		}
		foreach (SolutionItem item in this.folder.SolutionItems.Items)
		{
			SolutionItemNode.NewNode(Solution, item).AddTo(this);
		}
		base.Initialize();
	}

	public override void Delete()
	{
		ProjectService.RemoveSolutionFolder(folder.IdGuid);
		solution.Save();
	}

	public override void Copy()
	{
		throw new NotSupportedException();
	}

	public override void Cut()
	{
		DoPerformCut = true;
		ClipboardWrapper.SetDataObject(new DataObject(typeof(ISolutionFolder).ToString(), folder.IdGuid));
	}

	public static bool DoEnablePaste(ISolutionFolderNode container)
	{
		IDataObject dataObject = ClipboardWrapper.GetDataObject();
		if (dataObject == null)
		{
			return false;
		}
		if (dataObject.GetDataPresent(typeof(ISolutionFolder).ToString()))
		{
			string guid = dataObject.GetData(typeof(ISolutionFolder).ToString()).ToString();
			ISolutionFolder solutionFolder = container.Solution.GetSolutionFolder(guid);
			if (solutionFolder == null || solutionFolder == container)
			{
				return false;
			}
			if (solutionFolder is ISolutionFolderContainer)
			{
				if (solutionFolder.Parent != container)
				{
					return !((ISolutionFolderContainer)solutionFolder).IsAncestorOf(container.Folder);
				}
				return false;
			}
			return solutionFolder.Parent != container;
		}
		return false;
	}

	public static void DoPaste(ISolutionFolderNode folderNode)
	{
		if (!DoEnablePaste(folderNode))
		{
			LoggingService.Warn("SolutionFolderNode.DoPaste: Pasting was not enabled.");
			return;
		}
		ExtTreeNode extTreeNode = (ExtTreeNode)folderNode;
		IDataObject dataObject = ClipboardWrapper.GetDataObject();
		if (dataObject.GetDataPresent(typeof(ISolutionFolder).ToString()))
		{
			string guid = dataObject.GetData(typeof(ISolutionFolder).ToString()).ToString();
			ISolutionFolder solutionFolder = folderNode.Solution.GetSolutionFolder(guid);
			if (solutionFolder != null)
			{
				folderNode.Container.AddFolder(solutionFolder);
				ExtTreeView extTreeView = (ExtTreeView)extTreeNode.TreeView;
				foreach (ExtTreeNode cutNode in extTreeView.CutNodes)
				{
					ExtTreeNode extTreeNode2 = cutNode.Parent as ExtTreeNode;
					cutNode.Remove();
					cutNode.AddTo(extTreeNode);
					extTreeNode2?.Refresh();
				}
				ProjectService.SaveSolution();
			}
		}
		extTreeNode.Expand();
	}

	public override void Paste()
	{
		DoPaste(this);
	}

	public override DragDropEffects GetDragDropEffect(IDataObject dataObject, DragDropEffects proposedEffect)
	{
		if (dataObject.GetDataPresent(typeof(SolutionFolderNode)))
		{
			SolutionFolderNode solutionFolderNode = (SolutionFolderNode)dataObject.GetData(typeof(SolutionFolderNode));
			if (solutionFolderNode.Folder.Parent != folder && !solutionFolderNode.Container.IsAncestorOf(Folder))
			{
				return DragDropEffects.Move;
			}
		}
		if (dataObject.GetDataPresent(typeof(SolutionItemNode)))
		{
			SolutionItemNode solutionItemNode = (SolutionItemNode)dataObject.GetData(typeof(SolutionItemNode));
			if (solutionItemNode.Parent != this)
			{
				return DragDropEffects.Move;
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
		if (!isInitialized)
		{
			Initialize();
			isInitialized = true;
		}
		if (dataObject.GetDataPresent(typeof(SolutionFolderNode)))
		{
			SolutionFolderNode solutionFolderNode = (SolutionFolderNode)dataObject.GetData(typeof(SolutionFolderNode));
			AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode = solutionFolderNode.Parent as AbstractProjectBrowserTreeNode;
			solutionFolderNode.Remove();
			solutionFolderNode.AddTo(this);
			solutionFolderNode.EnsureVisible();
			folder.AddFolder(solutionFolderNode.Folder);
			abstractProjectBrowserTreeNode?.Refresh();
		}
		if (dataObject.GetDataPresent(typeof(SolutionItemNode)))
		{
			SolutionItemNode solutionItemNode = (SolutionItemNode)dataObject.GetData(typeof(SolutionItemNode));
			ISolutionFolderNode solutionFolderNode2 = (ISolutionFolderNode)solutionItemNode.Parent;
			solutionFolderNode2.Container.SolutionItems.Items.Remove(solutionItemNode.SolutionItem);
			Container.SolutionItems.Items.Add(solutionItemNode.SolutionItem);
			solutionItemNode.Remove();
			solutionItemNode.AddTo(this);
			solutionItemNode.EnsureVisible();
			if (solutionItemNode.Parent != null)
			{
				((ExtTreeNode)solutionItemNode.Parent).Refresh();
			}
		}
		if (dataObject.GetDataPresent(typeof(ProjectNode)))
		{
			ProjectNode projectNode = (ProjectNode)dataObject.GetData(typeof(ProjectNode));
			projectNode.Remove();
			projectNode.AddTo(this);
			projectNode.EnsureVisible();
			folder.AddFolder(projectNode.Project);
			if (projectNode.Parent != null)
			{
				((ExtTreeNode)projectNode.Parent).Refresh();
			}
		}
		solution.Save();
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
	}
}
