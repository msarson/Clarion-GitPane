using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionItemNode : CustomFolderNode
{
	private Solution solution;

	private SolutionItem item;

	public static EventHandler<SolutionItemCreatingEventArgs> SolutionItemCreating;

	public SolutionItem SolutionItem => item;

	public string FileName => Path.Combine(solution.Directory, item.Location);

	public override DataObject DragDropDataObject => new DataObject(this);

	public override bool EnableDelete => true;

	public override bool EnablePaste => ((ExtTreeNode)base.Parent).EnablePaste;

	public override bool EnableCopy => true;

	public override bool EnableCut => true;

	protected SolutionItemNode(Solution solution, SolutionItem item)
	{
		sortOrder = 2;
		canLabelEdit = true;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/SolutionItemNode";
		this.solution = solution;
		this.item = item;
		base.Text = Path.GetFileName(FileName);
		SetIcon(IconService.GetImageForFile(FileName));
	}

	public static SolutionItemNode NewNode(Solution solution, SolutionItem item)
	{
		if (SolutionItemCreating != null)
		{
			SolutionItemCreatingEventArgs e = new SolutionItemCreatingEventArgs(solution, item);
			SolutionItemCreating(null, e);
			if (e.Node != null)
			{
				return e.Node;
			}
		}
		return new SolutionItemNode(solution, item);
	}

	public override void ActivateItem()
	{
		FileService.OpenFile(FileName);
	}

	public override DragDropEffects GetDragDropEffect(IDataObject dataObject, DragDropEffects proposedEffect)
	{
		return ((ExtTreeNode)base.Parent).GetDragDropEffect(dataObject, proposedEffect);
	}

	public override void DoDragDrop(IDataObject dataObject, DragDropEffects effect)
	{
		((ExtTreeNode)base.Parent).DoDragDrop(dataObject, effect);
	}

	public override void Delete()
	{
		TreeNode prevNode = base.PrevNode;
		ISolutionFolderNode solutionFolderNode = base.Parent as ISolutionFolderNode;
		solutionFolderNode.Container.SolutionItems.Items.Remove(item);
		Remove();
		if (item is IProject project && ProjectService.OpenSolution != null)
		{
			ProjectService.OpenSolution.RemoveProjectConfigurationPlatforms(project);
		}
		ProjectService.SaveSolution();
		SelectPreviousNode(prevNode);
	}

	public override void Paste()
	{
		((ExtTreeNode)base.Parent).Paste();
	}

	public override void Copy()
	{
		DoPerformCut = true;
		ClipboardWrapper.SetDataObject(FileOperationClipboardObject.CreateDataObject(this, performMove: false));
	}

	public override void Cut()
	{
		DoPerformCut = true;
		ClipboardWrapper.SetDataObject(FileOperationClipboardObject.CreateDataObject(this, performMove: true));
	}

	public override void AfterLabelEdit(string newName)
	{
		if (newName != null && FileService.CheckFileName(newName))
		{
			string newName2 = Path.Combine(Path.GetDirectoryName(FileName), newName);
			if (FileService.RenameFile(FileName, newName2, isDirectory: false))
			{
				solution.Save();
			}
		}
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
	}
}
