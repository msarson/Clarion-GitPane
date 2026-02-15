using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project.Commands;

namespace ICSharpCode.SharpDevelop.Project;

public class FileNode : AbstractProjectBrowserTreeNode, IOwnerState
{
	private string fileName = string.Empty;

	private FileNodeStatus fileNodeStatus = FileNodeStatus.None;

	private ProjectItem projectItem;

	public override bool Visible
	{
		get
		{
			if (!AbstractProjectBrowserTreeNode.ShowAll)
			{
				return fileNodeStatus != FileNodeStatus.None;
			}
			return true;
		}
	}

	public virtual string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
			base.Text = Path.GetFileName(fileName);
		}
	}

	public ProjectItem ProjectItem
	{
		get
		{
			return projectItem;
		}
		set
		{
			if (projectItem != value)
			{
				projectItem = value;
				base.Tag = projectItem;
				SetIcon();
			}
		}
	}

	public Enum InternalState => fileNodeStatus;

	public FileNodeStatus FileNodeStatus
	{
		get
		{
			return fileNodeStatus;
		}
		set
		{
			if (fileNodeStatus != value)
			{
				fileNodeStatus = value;
				SetIcon();
			}
		}
	}

	public bool IsLink
	{
		get
		{
			if (projectItem is FileProjectItem)
			{
				return (projectItem as FileProjectItem).IsLink;
			}
			return false;
		}
	}

	public virtual string RelativePath
	{
		get
		{
			if (base.Parent is DirectoryNode)
			{
				return Path.Combine(((DirectoryNode)base.Parent).RelativePath, base.Text);
			}
			return base.Text;
		}
	}

	public override DataObject DragDropDataObject => new DataObject(this);

	public override bool EnableDelete => true;

	public override bool EnableCopy
	{
		get
		{
			if (base.IsEditing)
			{
				return false;
			}
			return true;
		}
	}

	public override bool EnableCut
	{
		get
		{
			if (base.IsEditing)
			{
				return false;
			}
			return true;
		}
	}

	public override bool EnablePaste
	{
		get
		{
			if (base.IsEditing)
			{
				return false;
			}
			return ((ExtTreeNode)base.Parent).EnablePaste;
		}
	}

	private void SetIcon()
	{
		switch (fileNodeStatus)
		{
		case FileNodeStatus.None:
			SetIcon("ProjectBrowser.GhostFile");
			break;
		case FileNodeStatus.InProject:
			if (IsLink)
			{
				SetIcon("ProjectBrowser.CodeBehind");
			}
			else
			{
				SetIcon(IconService.GetImageForFile(FileName));
			}
			break;
		case FileNodeStatus.Missing:
			SetIcon("ProjectBrowser.MissingFile");
			break;
		case FileNodeStatus.BehindFile:
			SetIcon("ProjectBrowser.CodeBehind");
			break;
		}
	}

	public FileNode(string fileName, FileNodeStatus fileNodeStatus)
	{
		sortOrder = 5;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/FileNode";
		ToolbarAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ToolBar/File";
		this.fileNodeStatus = fileNodeStatus;
		FileName = fileName;
		autoClearNodes = false;
		SetIcon();
		canLabelEdit = true;
	}

	public FileNode(string fileName)
		: this(fileName, FileNodeStatus.None)
	{
		sortOrder = 5;
		canLabelEdit = true;
	}

	public override void ActivateItem()
	{
		FileService.OpenFile(FileName);
	}

	public override void AfterLabelEdit(string newName)
	{
		if (newName == null || !FileService.CheckFileName(newName))
		{
			return;
		}
		string text = FileName;
		if (text == null)
		{
			return;
		}
		string text2 = Path.Combine(Path.GetDirectoryName(text), newName);
		if (!FileService.RenameFile(text, text2, isDirectory: false))
		{
			return;
		}
		base.Text = newName;
		fileName = text2;
		string text3 = Path.GetFileNameWithoutExtension(text) + ".";
		string text4 = Path.GetFileNameWithoutExtension(text2) + ".";
		foreach (TreeNode node in base.Nodes)
		{
			if (node is FileNode fileNode)
			{
				if (fileNode.ProjectItem is FileProjectItem fileProjectItem && string.Equals(fileProjectItem.DependentUpon, Path.GetFileName(text), StringComparison.OrdinalIgnoreCase))
				{
					fileProjectItem.DependentUpon = newName;
				}
				if (fileNode.Text.StartsWith(text3))
				{
					fileNode.AfterLabelEdit(text4 + fileNode.Text.Substring(text3.Length));
				}
			}
			else
			{
				LoggingService.Warn("FileNode.AfterLabelEdit. Child is not a FileNode.");
			}
		}
		Project.Save();
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
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
		bool flag = false;
		TreeNode prevNode = base.PrevNode;
		if (base.Nodes.Count > 0)
		{
			if (MessageService.AskQuestion(GetQuestionText("${res:ProjectComponent.ContextMenu.DeleteWithDependentFiles.Question}"), StringParser.Parse("${res:Global.WarningText}"), defaultToYes: false))
			{
				DeleteChildNodes();
				FileService.RemoveFile(FileName, isDirectory: false);
				Project.Save();
				flag = true;
			}
		}
		else if (!File.Exists(FileName))
		{
			ExcludeFileFromProject.ExcludeFileNode(this);
			Remove();
			Project.Save();
			flag = true;
		}
		else if (MessageService.AskQuestion(GetQuestionText("${res:ProjectComponent.ContextMenu.Delete.Question}"), StringParser.Parse("${res:Global.WarningText}"), defaultToYes: false))
		{
			FileService.RemoveFile(FileName, isDirectory: false);
			if (IsLink)
			{
				ExcludeFileFromProject.ExcludeFileNode(this);
			}
			Project.Save();
			flag = true;
		}
		if (flag)
		{
			SelectPreviousNode(prevNode);
		}
	}

	public override void Copy()
	{
		ClipboardWrapper.SetDataObject(FileOperationClipboardObject.CreateDataObject(this, performMove: false));
	}

	public override void Cut()
	{
		DoPerformCut = true;
		ClipboardWrapper.SetDataObject(FileOperationClipboardObject.CreateDataObject(this, performMove: true));
	}

	public override void Paste()
	{
		((ExtTreeNode)base.Parent).Paste();
	}

	private void DeleteChildNodes()
	{
		if (base.Nodes.Count == 0)
		{
			return;
		}
		foreach (TreeNode node in base.Nodes)
		{
			if (node is FileNode fileNode)
			{
				fileNode.DeleteChildNodes();
				FileService.RemoveFile(fileNode.FileName, isDirectory: false);
			}
			else
			{
				LoggingService.Warn("FileNode.DeleteChildren. Child is not a FileNode.");
			}
		}
	}
}
