using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class ProjectBrowserControl : UserControl, IHasPropertyContainer
{
	private TreeViewLocator Locator;

	private IContainer components;

	private ExtTreeView treeView;

	private string lastSelectionTarget;

	private PropertyContainer propertyContainer = new PropertyContainer();

	public bool ShowAll
	{
		get
		{
			return AbstractProjectBrowserTreeNode.ShowAll;
		}
		set
		{
			if (AbstractProjectBrowserTreeNode.ShowAll == value)
			{
				return;
			}
			treeView.BeginUpdate();
			AbstractProjectBrowserTreeNode.ShowAll = value;
			foreach (AbstractProjectBrowserTreeNode node in treeView.Nodes)
			{
				node.UpdateVisibility();
			}
			treeView.Sort();
			treeView.EndUpdate();
		}
	}

	public AbstractProjectBrowserTreeNode SelectedNode => treeView.SelectedNode as AbstractProjectBrowserTreeNode;

	public AbstractProjectBrowserTreeNode RootNode
	{
		get
		{
			if (treeView.Nodes.Count > 0)
			{
				return treeView.Nodes[0] as AbstractProjectBrowserTreeNode;
			}
			return null;
		}
	}

	public ExtTreeView TreeView => treeView;

	public PropertyContainer PropertyContainer => propertyContainer;

	public void CollapseAll()
	{
		if (treeView != null)
		{
			treeView.CollapseAll();
		}
	}

	public void ExpandAll()
	{
		if (treeView != null)
		{
			treeView.ExpandAll();
		}
	}

	public ProjectBrowserControl()
	{
		InitializeComponent();
		treeView.CanClearSelection = false;
		treeView.BeforeSelect += TreeViewBeforeSelect;
		treeView.AfterExpand += TreeViewAfterExpand;
		FileService.FileRenamed += FileServiceFileRenamed;
		FileService.FileRemoved += FileServiceFileRemoved;
		ProjectService.ProjectItemAdded += ProjectServiceProjectItemAdded;
		ProjectService.SolutionFolderRemoved += ProjectServiceSolutionFolderRemoved;
		treeView.DrawNode += TreeViewDrawNode;
		treeView.DragDrop += TreeViewDragDrop;
	}

	private void TreeViewDragDrop(object sender, DragEventArgs e)
	{
		Point pt = PointToClient(new Point(e.X, e.Y));
		ExtTreeNode extTreeNode = treeView.GetNodeAt(pt) as ExtTreeNode;
		if (extTreeNode != null || !e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			return;
		}
		string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
		string[] array2 = array;
		foreach (string text in array2)
		{
			try
			{
				IProjectLoader projectLoader = ProjectService.GetProjectLoader(text);
				if (projectLoader != null)
				{
					FileUtility.ObservedLoad(projectLoader.Load, text);
				}
				else
				{
					FileService.OpenFile(text);
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex, "unable to open file " + text);
			}
		}
	}

	private void TreeViewDrawNode(object sender, DrawTreeNodeEventArgs e)
	{
		if (e.Node is AbstractProjectBrowserTreeNode { Overlay: { } overlay })
		{
			Graphics graphics = e.Graphics;
			graphics.DrawImageUnscaled(overlay, e.Bounds.X - overlay.Width, e.Bounds.Bottom - overlay.Height);
		}
	}

	private void CallVisitor(ProjectBrowserTreeNodeVisitor visitor)
	{
		foreach (AbstractProjectBrowserTreeNode node in treeView.Nodes)
		{
			node.AcceptVisitor(visitor, null);
		}
	}

	private void ProjectServiceSolutionFolderRemoved(object sender, SolutionFolderEventArgs e)
	{
		CallVisitor(new SolutionFolderRemoveVisitor(e.SolutionFolder));
	}

	private void ProjectServiceProjectItemAdded(object sender, ProjectItemEventArgs e)
	{
		if (e.ProjectItem is ReferenceProjectItem)
		{
			CallVisitor(new UpdateReferencesVisitor(e));
		}
	}

	private void FileServiceFileRemoved(object sender, FileEventArgs e)
	{
		CallVisitor(new FileRemoveTreeNodeVisitor(e.FileName));
	}

	private void FileServiceFileRenamed(object sender, FileRenameEventArgs e)
	{
		CallVisitor(new FileRenameTreeNodeVisitor(e.SourceFile, e.TargetFile));
	}

	public void RefreshViewOpenSolution()
	{
		ViewOpenSolution();
	}

	public void RefreshView()
	{
		RefreshView(keepState: true);
	}

	public void RefreshView(bool keepState)
	{
		if (treeView.Nodes.Count > 0)
		{
			Properties memento = null;
			if (keepState)
			{
				memento = new Properties();
				StoreViewState(memento);
			}
			ViewSolution(((AbstractProjectBrowserTreeNode)treeView.Nodes[0]).Solution);
			if (keepState)
			{
				ReadViewState(memento);
			}
		}
	}

	private FileNode FindFileNode(TreeNodeCollection nodes, string fileName)
	{
		foreach (TreeNode node in nodes)
		{
			if (node is FileNode fileNode && FileUtility.IsEqualFileName(fileNode.FileName, fileName))
			{
				return fileNode;
			}
			if (node != null)
			{
				FileNode fileNode2 = FindFileNode(node.Nodes, fileName);
				if (fileNode2 != null)
				{
					return fileNode2;
				}
			}
		}
		return null;
	}

	public FileNode FindFileNode(string fileName)
	{
		return FindFileNode(treeView.Nodes, fileName);
	}

	public void SelectFile(string fileName)
	{
		lastSelectionTarget = fileName;
		TreeNode treeNode = FindFileNode(fileName);
		if (treeNode != null)
		{
			TreeNode treeNode2 = treeNode;
			for (TreeNode treeNode3 = treeNode.Parent; treeNode3 != null; treeNode3 = treeNode3.Parent)
			{
				if (!treeNode3.IsExpanded)
				{
					treeNode2 = treeNode3;
				}
			}
			if (treeNode2 != null)
			{
				treeView.SelectedNode = treeNode2;
			}
		}
		else
		{
			SelectDeepestOpenNodeForPath(fileName);
		}
	}

	private void SelectDeepestOpenNodeForPath(string fileName)
	{
		TreeNode treeNode = FindDeepestOpenNodeForPath(fileName);
		if (treeNode != null)
		{
			treeView.SelectedNode = treeNode;
		}
	}

	private TreeNode FindDeepestOpenNodeForPath(string fileName)
	{
		Solution openSolution = ProjectService.OpenSolution;
		if (openSolution == null)
		{
			return null;
		}
		IProject project = openSolution.FindProjectContainingFile(fileName);
		if (project == null)
		{
			return FindNoneProjectNodeByName(fileName);
		}
		string text = string.Empty;
		TreeNode treeNode = FindProjectNode(project);
		if (treeNode == null)
		{
			if (treeView.Nodes == null || treeView.Nodes.Count < 1)
			{
				return null;
			}
			treeNode = treeView.Nodes[0];
			if (fileName.StartsWith(openSolution.Directory))
			{
				text = fileName.Replace(openSolution.Directory, "");
			}
		}
		else
		{
			TreeNode treeNode2 = treeNode;
			for (TreeNode treeNode3 = treeNode.Parent; treeNode3 != null; treeNode3 = treeNode3.Parent)
			{
				if (!treeNode3.IsExpanded)
				{
					treeNode2 = treeNode3;
				}
			}
			if (treeNode2 != treeNode)
			{
				return treeNode2;
			}
			if (fileName.StartsWith((treeNode as ProjectNode).Directory))
			{
				text = fileName.Replace((treeNode as ProjectNode).Directory, "");
			}
		}
		if (!treeNode.IsExpanded)
		{
			return treeNode;
		}
		string[] array = text.Trim('/', '\\').Split('/', '\\');
		TreeNode treeNode4 = null;
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			treeNode4 = null;
			foreach (TreeNode node in treeNode.Nodes)
			{
				if (node != null && node.Text == text2)
				{
					treeNode4 = node;
					break;
				}
			}
			if (treeNode4 == null)
			{
				break;
			}
			treeNode = treeNode4;
		}
		return treeNode;
	}

	private ProjectNode FindProjectNode(IProject project)
	{
		if (project == null)
		{
			return null;
		}
		return FindProjectNodeByName(treeView.Nodes, project.Name);
	}

	public TreeNode FindNoneProjectNodeByName(string nodeName)
	{
		return FindNoneProjectNodeByName(treeView.Nodes, nodeName);
	}

	private TreeNode FindNoneProjectNodeByName(TreeNodeCollection nodes, string nodeName)
	{
		if (nodes == null)
		{
			return null;
		}
		foreach (TreeNode node in nodes)
		{
			if (node != null && !(node is ProjectNode))
			{
				if (node.Text == nodeName)
				{
					return node;
				}
				TreeNode treeNode2 = FindNoneProjectNodeByName(node.Nodes, nodeName);
				if (treeNode2 != null && treeNode2.Text == nodeName)
				{
					return treeNode2;
				}
			}
		}
		return null;
	}

	private ProjectNode FindProjectNodeByName(TreeNodeCollection nodes, string projectName)
	{
		if (nodes == null)
		{
			return null;
		}
		foreach (TreeNode node in nodes)
		{
			if (node != null)
			{
				if (node is ProjectNode projectNode && projectNode.Text == projectName)
				{
					return projectNode;
				}
				ProjectNode projectNode2 = FindProjectNodeByName(node.Nodes, projectName);
				if (projectNode2 != null)
				{
					return projectNode2;
				}
			}
		}
		return null;
	}

	public void ViewOpenSolution()
	{
		if (ProjectService.OpenSolution != null)
		{
			ViewSolution(ProjectService.OpenSolution);
		}
	}

	public void ViewSolution(Solution solution)
	{
		AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode = new SolutionNode(solution);
		treeView.Clear();
		abstractProjectBrowserTreeNode.AddTo(treeView);
		foreach (ISolutionFolder folder in solution.Folders)
		{
			if (folder is IProject)
			{
				NodeBuilders.AddProjectNode(abstractProjectBrowserTreeNode, (IProject)folder);
				continue;
			}
			SolutionFolderNode solutionFolderNode = new SolutionFolderNode(solution, (SolutionFolder)folder);
			solutionFolderNode.AddTo(abstractProjectBrowserTreeNode);
		}
		abstractProjectBrowserTreeNode.Expand();
	}

	public void Clear()
	{
		treeView.Clear();
		propertyContainer.Clear();
	}

	public void PadActivated()
	{
		TreeViewBeforeSelect(null, new TreeViewCancelEventArgs(treeView.SelectedNode, cancel: false, TreeViewAction.Unknown));
	}

	private void TreeViewAfterExpand(object sender, TreeViewEventArgs e)
	{
		if (lastSelectionTarget == null)
		{
			return;
		}
		for (TreeNode treeNode = FindDeepestOpenNodeForPath(lastSelectionTarget); treeNode != null; treeNode = treeNode.Parent)
		{
			if (treeNode.Parent == e.Node)
			{
				treeView.SelectedNode = treeNode;
				break;
			}
		}
	}

	private void TreeViewBeforeSelect(object sender, TreeViewCancelEventArgs e)
	{
		if (e.Node is AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode)
		{
			ProjectService.CurrentProject = abstractProjectBrowserTreeNode.Project;
			List<object> list = new List<object>();
			if (abstractProjectBrowserTreeNode.Tag is IComponent)
			{
				IComponent component = (IComponent)abstractProjectBrowserTreeNode.Tag;
				ProjectNodeSite.Instance.Name = abstractProjectBrowserTreeNode.Text;
				ProjectNodeSite.Instance.Component = component;
				component.Site = ProjectNodeSite.Instance;
				list.Add(abstractProjectBrowserTreeNode.Tag);
				propertyContainer.SelectableObjects = list;
			}
			else
			{
				propertyContainer.SelectableObjects = null;
			}
			propertyContainer.SelectedObject = abstractProjectBrowserTreeNode.Tag;
		}
	}

	public void StoreViewState(Properties memento)
	{
		memento.Set("ProjectBrowserState", ExtTreeView.GetViewStateString(treeView));
	}

	public void ReadViewState(Properties memento)
	{
		ExtTreeView.ApplyViewStateString(memento.Get("ProjectBrowserState", ""), treeView);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.F1)
		{
			string parameter = "Project_Pad.htm";
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			FileInfo fileInfo = new FileInfo(entryAssembly.Location);
			string text = Path.Combine(fileInfo.DirectoryName, "ClarionHelp.chm");
			if (File.Exists(text))
			{
				Help.ShowHelp(WorkbenchSingleton.helpHost, text, HelpNavigator.Topic, parameter);
			}
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		ICSharpCode.SharpDevelop.Gui.ExtTreeViewComparer nodeSorter = new ICSharpCode.SharpDevelop.Gui.ExtTreeViewComparer();
		this.treeView = new ICSharpCode.SharpDevelop.Gui.ExtTreeView();
		this.Locator = new ICSharpCode.SharpDevelop.Gui.TreeViewLocator();
		base.SuspendLayout();
		this.treeView.ActivateItemOnDoubleClick = true;
		this.treeView.ActivateItemOnEnterKeyPress = true;
		this.treeView.AllowDrop = true;
		this.treeView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.treeView.CanClearSelection = true;
		this.Locator.SetDoLocate(this.treeView, true);
		this.treeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
		this.treeView.HideSelection = false;
		this.treeView.ImageIndex = 0;
		this.treeView.IsSorted = true;
		this.treeView.Location = new System.Drawing.Point(4, 31);
		this.treeView.Name = "treeView";
		this.treeView.NodeSorter = nodeSorter;
		this.treeView.SelectedImageIndex = 0;
		this.treeView.Size = new System.Drawing.Size(283, 232);
		this.treeView.TabIndex = 0;
		this.Locator.BackColor = System.Drawing.SystemColors.Control;
		this.Locator.Dock = System.Windows.Forms.DockStyle.Top;
		this.Locator.InString = true;
		this.Locator.IsTransparent = false;
		this.Locator.Location = new System.Drawing.Point(0, 0);
		this.Locator.Margin = new System.Windows.Forms.Padding(0);
		this.Locator.MaximumSize = new System.Drawing.Size(1384, 28);
		this.Locator.MinimumSize = new System.Drawing.Size(124, 28);
		this.Locator.Name = "Locator";
		this.Locator.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.Locator.ShowBeginWithButton = false;
		this.Locator.Size = new System.Drawing.Size(292, 28);
		this.Locator.TabIndex = 0;
		this.Locator.TreeToSearch = this.treeView;
		base.Controls.Add(this.treeView);
		base.Controls.Add(this.Locator);
		base.Name = "ProjectBrowserControl";
		base.Size = new System.Drawing.Size(292, 266);
		base.ResumeLayout(false);
	}
}
