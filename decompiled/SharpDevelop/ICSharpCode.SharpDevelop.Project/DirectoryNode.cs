using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project.Commands;

namespace ICSharpCode.SharpDevelop.Project;

public class DirectoryNode : AbstractProjectBrowserTreeNode, IOwnerState
{
	private string closedImage;

	private string openedImage;

	private FileNodeStatus fileNodeStatus = FileNodeStatus.None;

	private SpecialFolder specialFolder;

	private ProjectItem projectItem;

	private string directory = string.Empty;

	private CustomNode removeMe;

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

	public SpecialFolder SpecialFolder
	{
		get
		{
			return specialFolder;
		}
		set
		{
			if (specialFolder != value)
			{
				specialFolder = value;
				SetIcon();
			}
		}
	}

	public string ClosedImage
	{
		get
		{
			return closedImage;
		}
		set
		{
			closedImage = value;
			if (!base.IsExpanded)
			{
				SetIcon(closedImage);
			}
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
			projectItem = value;
			if (projectItem != null && projectItem.ItemType == ItemType.WebReferenceUrl)
			{
				base.Tag = projectItem;
			}
		}
	}

	public string OpenedImage
	{
		get
		{
			return openedImage;
		}
		set
		{
			openedImage = value;
			if (base.IsExpanded)
			{
				SetIcon(openedImage);
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

	public virtual string Directory
	{
		get
		{
			return directory;
		}
		set
		{
			directory = value;
			base.Text = Path.GetFileName(directory);
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

	public override bool EnableDelete => true;

	public override bool EnablePaste
	{
		get
		{
			IDataObject dataObject = ClipboardWrapper.GetDataObject();
			if (dataObject == null)
			{
				return false;
			}
			if (dataObject.GetDataPresent(DataFormats.FileDrop))
			{
				return true;
			}
			if (dataObject.GetDataPresent(typeof(FileNode)))
			{
				FileOperationClipboardObject fileOperationClipboardObject = (FileOperationClipboardObject)dataObject.GetData(typeof(FileNode).ToString());
				return File.Exists(fileOperationClipboardObject.FileName);
			}
			if (dataObject.GetDataPresent(typeof(DirectoryNode)))
			{
				FileOperationClipboardObject fileOperationClipboardObject2 = (FileOperationClipboardObject)dataObject.GetData(typeof(DirectoryNode).ToString());
				if (FileUtility.IsBaseDirectory(fileOperationClipboardObject2.FileName, Directory))
				{
					return false;
				}
				return System.IO.Directory.Exists(fileOperationClipboardObject2.FileName);
			}
			return false;
		}
	}

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

	public override DataObject DragDropDataObject => new DataObject(this);

	public override void Refresh()
	{
		base.Refresh();
		if (base.Nodes.Count == 0)
		{
			SetIcon(ClosedImage);
		}
		else if (base.IsExpanded)
		{
			SetIcon(openedImage);
		}
	}

	private void SetIcon()
	{
		switch (fileNodeStatus)
		{
		case FileNodeStatus.None:
			OpenedImage = "ProjectBrowser.GhostFolder.Open";
			ClosedImage = "ProjectBrowser.GhostFolder.Closed";
			return;
		case FileNodeStatus.Missing:
			OpenedImage = "ProjectBrowser.Folder.Missing";
			ClosedImage = "ProjectBrowser.Folder.Missing";
			return;
		}
		switch (SpecialFolder)
		{
		case SpecialFolder.None:
			OpenedImage = "ProjectBrowser.Folder.Open";
			ClosedImage = "ProjectBrowser.Folder.Closed";
			break;
		case SpecialFolder.AppDesigner:
			OpenedImage = "ProjectBrowser.PropertyFolder.Open";
			ClosedImage = "ProjectBrowser.PropertyFolder.Closed";
			break;
		case SpecialFolder.WebReferencesFolder:
			OpenedImage = "ProjectBrowser.WebReferenceFolder.Open";
			ClosedImage = "ProjectBrowser.WebReferenceFolder.Closed";
			break;
		case SpecialFolder.WebReference:
			OpenedImage = "ProjectBrowser.WebReference";
			ClosedImage = "ProjectBrowser.WebReference";
			break;
		}
	}

	protected DirectoryNode()
	{
		sortOrder = 1;
		SetIcon();
		canLabelEdit = true;
	}

	public DirectoryNode(string directory)
		: this(directory, FileNodeStatus.None)
	{
		sortOrder = 1;
		canLabelEdit = true;
	}

	public DirectoryNode(string directory, FileNodeStatus fileNodeStatus)
	{
		sortOrder = 1;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/FolderNode";
		Directory = directory;
		this.fileNodeStatus = fileNodeStatus;
		removeMe = new CustomNode();
		removeMe.AddTo(this);
		SetIcon();
		canLabelEdit = true;
	}

	public static bool IsWebReferencesFolder(IProject project, string folder)
	{
		foreach (ProjectItem item in project.Items)
		{
			if (item.ItemType == ItemType.WebReferences && FileUtility.IsEqualFileName(Path.Combine(project.Directory, item.Include), folder))
			{
				return true;
			}
		}
		return false;
	}

	public void RecreateSubNodes()
	{
		invisibleNodes.Clear();
		if (autoClearNodes)
		{
			base.Nodes.Clear();
		}
		else
		{
			List<TreeNode> list = new List<TreeNode>();
			foreach (TreeNode node in base.Nodes)
			{
				if (node is FileNode || node is DirectoryNode)
				{
					list.Add(node);
				}
			}
			foreach (TreeNode item in list)
			{
				base.Nodes.Remove(item);
			}
		}
		Initialize();
		UpdateVisibility();
	}

	protected override void Initialize()
	{
		if (removeMe != null)
		{
			base.Nodes.Remove(removeMe);
			removeMe = null;
		}
		LoggingService.Info("Initialize DirectoryNode " + Directory);
		Dictionary<string, FileNode> dictionary = new Dictionary<string, FileNode>(StringComparer.InvariantCultureIgnoreCase);
		Dictionary<FileNode, string> dictionary2 = new Dictionary<FileNode, string>();
		Dictionary<string, DirectoryNode> dictionary3 = new Dictionary<string, DirectoryNode>(StringComparer.InvariantCultureIgnoreCase);
		if (System.IO.Directory.Exists(Directory))
		{
			string[] directories = System.IO.Directory.GetDirectories(Directory);
			foreach (string text in directories)
			{
				if (Path.GetFileName(text) != ".svn")
				{
					DirectoryNode directoryNode = CreateDirectoryNode(text);
					if (directoryNode != null)
					{
						directoryNode.AddTo(this);
						dictionary3[Path.GetFileName(text)] = directoryNode;
					}
				}
			}
			string[] files = System.IO.Directory.GetFiles(Directory);
			foreach (string text2 in files)
			{
				FileNode fileNode = CreateFileNode(text2);
				if (fileNode != null)
				{
					dictionary[Path.GetFileName(text2)] = fileNode;
					fileNode.AddTo(this);
				}
			}
		}
		if (base.Nodes.Count == 0)
		{
			SetClosedImage();
		}
		string text3 = RelativePath;
		if (text3.Length > 0)
		{
			text3 = text3.Replace('\\', '/') + '/';
		}
		foreach (ProjectItem item in Project.Items)
		{
			if (item.ItemType == ItemType.WebReferenceUrl)
			{
				if (dictionary3.TryGetValue(Path.GetFileName(item.FileName), out var value))
				{
					if (value.FileNodeStatus == FileNodeStatus.None)
					{
						value.FileNodeStatus = FileNodeStatus.InProject;
					}
					value.ProjectItem = item;
				}
			}
			else
			{
				if (!(item is FileProjectItem fileProjectItem))
				{
					continue;
				}
				string text4 = fileProjectItem.VirtualName.Replace('\\', '/');
				if (text4.EndsWith("/"))
				{
					text4 = text4.Substring(0, text4.Length - 1);
				}
				string fileName = Path.GetFileName(text4);
				if (!string.Equals(text4, text3 + fileName, StringComparison.InvariantCultureIgnoreCase))
				{
					AddParentFolder(text4, text3, dictionary3);
					continue;
				}
				if (item.ItemType == ItemType.Folder || item.ItemType == ItemType.WebReferences)
				{
					if (dictionary3.TryGetValue(fileName, out var value2))
					{
						if (value2.FileNodeStatus == FileNodeStatus.None)
						{
							value2.FileNodeStatus = FileNodeStatus.InProject;
						}
						value2.ProjectItem = item;
						continue;
					}
					value2 = CreateDirectoryNode(item, value2);
					if (value2 != null)
					{
						value2.AddTo(this);
						dictionary3[fileName] = value2;
					}
					continue;
				}
				FileNode value3;
				if (fileProjectItem.IsLink)
				{
					value3 = new FileNode(fileProjectItem.FileName, FileNodeStatus.InProject);
					value3.AddTo(this);
					dictionary[fileName] = value3;
				}
				else if (dictionary.TryGetValue(fileName, out value3))
				{
					if (value3.FileNodeStatus == FileNodeStatus.None)
					{
						value3.FileNodeStatus = FileNodeStatus.InProject;
					}
				}
				else
				{
					value3 = new FileNode(fileProjectItem.FileName, FileNodeStatus.Missing);
					value3.AddTo(this);
					dictionary[fileName] = value3;
				}
				value3.ProjectItem = fileProjectItem;
				if (fileProjectItem != null && fileProjectItem.DependentUpon != null && fileProjectItem.DependentUpon.Length > 0)
				{
					dictionary2[value3] = fileProjectItem.DependentUpon;
				}
			}
		}
		foreach (KeyValuePair<FileNode, string> item2 in dictionary2)
		{
			string fileName2 = Path.GetFileName(item2.Value);
			if (!dictionary.ContainsKey(fileName2))
			{
				continue;
			}
			AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode = dictionary[fileName2];
			item2.Key.Parent.Nodes.Remove(item2.Key);
			if (NodeIsParent(abstractProjectBrowserTreeNode, item2.Key))
			{
				item2.Key.AddTo(this);
				continue;
			}
			item2.Key.AddTo(abstractProjectBrowserTreeNode);
			if (item2.Key.FileNodeStatus != FileNodeStatus.Missing)
			{
				item2.Key.FileNodeStatus = FileNodeStatus.BehindFile;
			}
		}
		base.Initialize();
	}

	protected virtual FileNode CreateFileNode(string file)
	{
		return new FileNode(file);
	}

	protected virtual DirectoryNode CreateDirectoryNode(ProjectItem item, DirectoryNode node)
	{
		return DirectoryNodeFactory.CreateDirectoryNode(item, FileNodeStatus.Missing);
	}

	protected virtual DirectoryNode CreateDirectoryNode(string subDirectory)
	{
		return DirectoryNodeFactory.CreateDirectoryNode(this, Project, subDirectory);
	}

	private static bool NodeIsParent(TreeNode childNode, TreeNode parentNode)
	{
		do
		{
			if (childNode == parentNode)
			{
				return true;
			}
			childNode = childNode.Parent;
		}
		while (childNode != null);
		return false;
	}

	protected void BaseInitialize()
	{
		base.Initialize();
	}

	private void AddParentFolder(string virtualName, string relativeDirectoryPath, Dictionary<string, DirectoryNode> directoryNodeList)
	{
		if (relativeDirectoryPath.Length != 0 && string.Compare(virtualName, 0, relativeDirectoryPath, 0, relativeDirectoryPath.Length, StringComparison.InvariantCultureIgnoreCase) != 0)
		{
			return;
		}
		int num = virtualName.IndexOf('/', relativeDirectoryPath.Length + 1);
		if (num < 0)
		{
			return;
		}
		string text = virtualName.Substring(relativeDirectoryPath.Length, num - relativeDirectoryPath.Length);
		if (directoryNodeList.TryGetValue(text, out var value))
		{
			if (value.FileNodeStatus == FileNodeStatus.None)
			{
				value.FileNodeStatus = FileNodeStatus.InProject;
			}
		}
		else
		{
			value = new DirectoryNode(Path.Combine(Directory, text), FileNodeStatus.Missing);
			value.AddTo(this);
			directoryNodeList[text] = value;
		}
	}

	private void SetOpenedImage()
	{
		if (openedImage != null)
		{
			SetIcon(openedImage);
		}
	}

	private void SetClosedImage()
	{
		if (closedImage != null)
		{
			SetIcon(closedImage);
		}
	}

	public override void Expanding()
	{
		SetOpenedImage();
		base.Expanding();
	}

	public override void Collapsing()
	{
		SetClosedImage();
		base.Collapsing();
	}

	public override void AfterLabelEdit(string newName)
	{
		if (newName == null || !FileService.CheckFileName(newName) || !FileService.CheckDirectoryName(newName) || string.Compare(base.Text, newName, ignoreCase: true) == 0)
		{
			return;
		}
		string text = base.Text;
		base.Text = newName;
		if (Directory == null)
		{
			return;
		}
		string text2 = Path.Combine(Path.GetDirectoryName(Directory), newName);
		if (System.IO.Directory.Exists(text2))
		{
			if (System.IO.Directory.GetFileSystemEntries(text2).Length != 0)
			{
				MessageService.ShowError("The folder already exists and contains files!");
				base.Text = text;
				return;
			}
			System.IO.Directory.Delete(text2);
			FileService.RenameFile(Directory, text2, isDirectory: true);
		}
		else
		{
			FileService.RenameFile(Directory, text2, isDirectory: true);
		}
		directory = text2;
		Project.Save();
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
	}

	public override void Delete()
	{
		TreeNode prevNode = base.PrevNode;
		bool flag = false;
		if (FileNodeStatus == FileNodeStatus.Missing)
		{
			FileService.RemoveFile(Directory, isDirectory: true);
			Project.Save();
			flag = true;
		}
		else if (MessageService.AskQuestion(GetQuestionText("${res:ProjectComponent.ContextMenu.DeleteWithContents.Question}"), StringParser.Parse("${res:Global.WarningText}"), defaultToYes: false))
		{
			FileService.RemoveFile(Directory, isDirectory: true);
			Project.Save();
			flag = true;
		}
		if (flag)
		{
			SelectPreviousNode(prevNode);
		}
	}

	public override void Paste()
	{
		IDataObject dataObject = ClipboardWrapper.GetDataObject();
		if (dataObject.GetDataPresent(DataFormats.FileDrop))
		{
			string[] array = (string[])dataObject.GetData(DataFormats.FileDrop);
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (System.IO.Directory.Exists(text))
				{
					if (!FileUtility.IsBaseDirectory(text, Directory))
					{
						CopyDirectoryHere(text, performMove: false);
					}
				}
				else
				{
					CopyFileHere(text, performMove: false);
				}
			}
		}
		else if (dataObject.GetDataPresent(typeof(FileNode)))
		{
			FileOperationClipboardObject fileOperationClipboardObject = (FileOperationClipboardObject)dataObject.GetData(typeof(FileNode).ToString());
			if (File.Exists(fileOperationClipboardObject.FileName))
			{
				CopyFileHere(fileOperationClipboardObject.FileName, fileOperationClipboardObject.PerformMove);
				if (fileOperationClipboardObject.PerformMove)
				{
					Clipboard.Clear();
				}
			}
		}
		else if (dataObject.GetDataPresent(typeof(DirectoryNode)))
		{
			FileOperationClipboardObject fileOperationClipboardObject2 = (FileOperationClipboardObject)dataObject.GetData(typeof(DirectoryNode).ToString());
			if (System.IO.Directory.Exists(fileOperationClipboardObject2.FileName))
			{
				CopyDirectoryHere(fileOperationClipboardObject2.FileName, fileOperationClipboardObject2.PerformMove);
				if (fileOperationClipboardObject2.PerformMove)
				{
					Clipboard.Clear();
				}
			}
		}
		ProjectService.SaveSolution();
	}

	public void CopyDirectoryHere(string directoryName, bool performMove)
	{
		string fileName = Path.Combine(Directory, Path.GetFileName(directoryName));
		if (FileUtility.IsEqualFileName(directoryName, fileName))
		{
			return;
		}
		AddExistingItemsToProject.CopyDirectory(directoryName, this, includeInProject: true);
		if (!performMove)
		{
			return;
		}
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			if (item.FileName != null && FileUtility.IsBaseDirectory(directoryName, item.FileName))
			{
				item.FileName = FileUtility.RenameBaseDirectory(item.FileName, directoryName, Path.Combine(directory, Path.GetFileName(directoryName)));
			}
		}
		FileService.RemoveFile(directoryName, isDirectory: true);
	}

	public void CopyDirectoryHere(DirectoryNode node, bool performMove)
	{
		CopyDirectoryHere(node.Directory, performMove);
	}

	public void CopyFileHere(string fileName, bool performMove)
	{
		string fileName2 = Path.GetFileName(fileName);
		string text = Path.Combine(Directory, fileName2);
		if (FileUtility.IsEqualFileName(fileName, text))
		{
			return;
		}
		bool flag = false;
		if (File.Exists(text))
		{
			if (!FileService.FireFileReplacing(text, isDirectory: false) || AddExistingItemsToProject.ShowReplaceExistingFileDialog(null, text, replacingMultiple: false) != AddExistingItemsToProject.ReplaceExistingFile.Yes)
			{
				return;
			}
			flag = true;
		}
		FileProjectItem fileProjectItem = AddExistingItemsToProject.CopyFile(fileName, this, includeInProject: true);
		IProject project = Solution.FindProjectContainingFile(fileName);
		if (project != null)
		{
			string directoryName = Path.GetDirectoryName(fileName);
			bool flag2 = false;
			foreach (ProjectItem item in project.Items)
			{
				if (!(item is FileProjectItem fileProjectItem2))
				{
					continue;
				}
				if (fileProjectItem != null && FileUtility.IsEqualFileName(fileProjectItem2.FileName, fileName))
				{
					fileProjectItem2.CopyMetadataTo(fileProjectItem);
				}
				if (string.Equals(fileProjectItem2.DependentUpon, fileName2, StringComparison.OrdinalIgnoreCase))
				{
					string text2 = Path.Combine(project.Directory, fileProjectItem2.VirtualName);
					if (FileUtility.IsEqualFileName(directoryName, Path.GetDirectoryName(text2)))
					{
						CopyFileHere(text2, performMove);
						flag2 = true;
					}
				}
			}
			if (flag2)
			{
				RecreateSubNodes();
			}
		}
		if (performMove)
		{
			foreach (IViewContent item2 in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				if (item2.FileName != null && FileUtility.IsEqualFileName(item2.FileName, fileName))
				{
					item2.FileName = text;
					item2.TitleName = fileName2;
				}
			}
			FileService.RemoveFile(fileName, isDirectory: false);
		}
		if (flag)
		{
			FileService.FireFileReplaced(text, isDirectory: false);
		}
	}

	public void CopyFileHere(FileNode node, bool performMove)
	{
		if (node.FileNodeStatus == FileNodeStatus.None)
		{
			AddExistingItemsToProject.CopyFile(node.FileName, this, includeInProject: false);
			if (performMove)
			{
				FileService.RemoveFile(node.FileName, isDirectory: false);
			}
		}
		else if (node.IsLink)
		{
			string relativePath = FileUtility.GetRelativePath(Project.Directory, node.FileName);
			FileNode fileNode = new FileNode(node.FileName, FileNodeStatus.InProject);
			FileProjectItem fileProjectItem = new FileProjectItem(Project, Project.GetDefaultItemType(node.FileName));
			fileProjectItem.Include = relativePath;
			fileProjectItem.SetEvaluatedMetadata("Link", Path.Combine(RelativePath, Path.GetFileName(node.FileName)));
			fileNode.ProjectItem = fileProjectItem;
			fileNode.AddTo(this);
			ProjectService.AddProjectItem(Project, fileProjectItem);
			if (performMove)
			{
				ProjectService.RemoveProjectItem(node.Project, node.ProjectItem);
				node.Remove();
			}
		}
		else
		{
			CopyFileHere(node.FileName, performMove);
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

	public override DragDropEffects GetDragDropEffect(IDataObject dataObject, DragDropEffects proposedEffect)
	{
		if (dataObject.GetDataPresent(typeof(FileNode)))
		{
			FileNode fileNode = (FileNode)dataObject.GetData(typeof(FileNode));
			if (!FileUtility.IsEqualFileName(Directory, fileNode.FileName) && !FileUtility.IsEqualFileName(Directory, Path.GetDirectoryName(fileNode.FileName)))
			{
				if (Project != fileNode.Project)
				{
					return DragDropEffects.Copy;
				}
				return proposedEffect;
			}
		}
		if (dataObject.GetDataPresent(typeof(DirectoryNode)))
		{
			DirectoryNode directoryNode = (DirectoryNode)dataObject.GetData(typeof(DirectoryNode));
			if (FileUtility.IsBaseDirectory(directoryNode.Directory, Directory))
			{
				return DragDropEffects.None;
			}
			if (!FileUtility.IsEqualFileName(Directory, directoryNode.Directory) && !FileUtility.IsEqualFileName(Directory, Path.GetDirectoryName(directoryNode.Directory)))
			{
				if (Project != directoryNode.Project)
				{
					return DragDropEffects.Copy;
				}
				return proposedEffect;
			}
		}
		if (dataObject.GetDataPresent(DataFormats.FileDrop))
		{
			return DragDropEffects.Copy;
		}
		return DragDropEffects.None;
	}

	public override void DoDragDrop(IDataObject dataObject, DragDropEffects effect)
	{
		PerformInitialization();
		Expand();
		try
		{
			if (dataObject.GetDataPresent(typeof(FileNode)))
			{
				FileNode node = (FileNode)dataObject.GetData(typeof(FileNode));
				CopyFileHere(node, effect == DragDropEffects.Move);
			}
			else if (dataObject.GetDataPresent(typeof(DirectoryNode)))
			{
				DirectoryNode node2 = (DirectoryNode)dataObject.GetData(typeof(DirectoryNode));
				CopyDirectoryHere(node2, effect == DragDropEffects.Move);
			}
			else if (dataObject.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = (string[])dataObject.GetData(DataFormats.FileDrop);
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (System.IO.Directory.Exists(text))
					{
						if (!FileUtility.IsBaseDirectory(text, Directory))
						{
							CopyDirectoryHere(text, performMove: false);
						}
					}
					else
					{
						CopyFileHere(text, performMove: false);
					}
				}
			}
			ProjectService.SaveSolution();
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}
}
