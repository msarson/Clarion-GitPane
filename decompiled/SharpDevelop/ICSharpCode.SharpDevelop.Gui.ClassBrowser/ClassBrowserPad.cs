using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ClassBrowserPad : AbstractPadContent
{
	private static ClassBrowserPad instance;

	private ClassBrowserFilter filter = ClassBrowserFilter.All;

	private Panel contentPanel = new Panel();

	private ExtTreeView classBrowserTreeView = new ExtTreeView();

	private ToolStrip toolStrip;

	private ToolStrip searchStrip;

	private List<ICompilationUnit[]> pending = new List<ICompilationUnit[]>();

	private Stack<TreeNode> previousNodes = new Stack<TreeNode>();

	private Stack<TreeNode> nextNodes = new Stack<TreeNode>();

	private bool navigateBack;

	private bool navigateForward;

	private bool inSearchMode;

	private List<TreeNode> oldNodes = new List<TreeNode>();

	private string searchTerm = "";

	public static ClassBrowserPad Instance => instance;

	public ClassBrowserFilter Filter
	{
		get
		{
			return filter;
		}
		set
		{
			filter = value;
			foreach (TreeNode node in classBrowserTreeView.Nodes)
			{
				if (node is ExtTreeNode)
				{
					((ExtTreeNode)node).UpdateVisibility();
				}
			}
			classBrowserTreeView.SortNodes(classBrowserTreeView.Nodes, recursive: true);
		}
	}

	public override Control Control => contentPanel;

	public bool CanNavigateBackward
	{
		get
		{
			if (previousNodes.Count == 1 && classBrowserTreeView.SelectedNode == previousNodes.Peek())
			{
				return false;
			}
			return previousNodes.Count > 0;
		}
	}

	public bool CanNavigateForward
	{
		get
		{
			if (nextNodes.Count == 1 && classBrowserTreeView.SelectedNode == nextNodes.Peek())
			{
				return false;
			}
			return nextNodes.Count > 0;
		}
	}

	public bool IsInSearchMode => inSearchMode;

	public string SearchTerm
	{
		get
		{
			return searchTerm;
		}
		set
		{
			searchTerm = value.ToUpper().Trim();
		}
	}

	private void UpdateToolbars()
	{
		ToolbarService.UpdateToolbar(toolStrip);
		ToolbarService.UpdateToolbar(searchStrip);
	}

	public ClassBrowserPad()
	{
		instance = this;
		classBrowserTreeView.Dock = DockStyle.Fill;
		classBrowserTreeView.ImageList = ClassBrowserIconService.ImageList;
		classBrowserTreeView.AfterSelect += ClassBrowserTreeViewAfterSelect;
		contentPanel.Controls.Add(classBrowserTreeView);
		searchStrip = ToolbarService.CreateToolStrip(this, "/SharpDevelop/Pads/ClassBrowser/Searchbar");
		searchStrip.Stretch = true;
		searchStrip.GripStyle = ToolStripGripStyle.Hidden;
		contentPanel.Controls.Add(searchStrip);
		toolStrip = ToolbarService.CreateToolStrip(this, "/SharpDevelop/Pads/ClassBrowser/Toolbar");
		toolStrip.Stretch = true;
		toolStrip.GripStyle = ToolStripGripStyle.Hidden;
		contentPanel.Controls.Add(toolStrip);
		ProjectService.SolutionLoaded += ProjectServiceSolutionChanged;
		ProjectService.ProjectAdded += ProjectServiceSolutionChanged;
		ProjectService.SolutionFolderRemoved += ProjectServiceSolutionChanged;
		ProjectService.SolutionClosed += ProjectServiceSolutionClosed;
		ParserService.ParseInformationUpdated += ParserServiceParseInformationUpdated;
		AmbienceService.AmbienceChanged += AmbienceServiceAmbienceChanged;
		if (ProjectService.OpenSolution != null)
		{
			ProjectServiceSolutionChanged(null, null);
		}
		UpdateToolbars();
	}

	private void UpdateThread()
	{
		lock (pending)
		{
			foreach (ICompilationUnit[] item in pending)
			{
				ICompilationUnit compilationUnit = item[1] ?? item[0];
				foreach (TreeNode node in classBrowserTreeView.Nodes)
				{
					if (node is AbstractProjectNode abstractProjectNode)
					{
						if (abstractProjectNode.Project.IsFileInProject(compilationUnit.FileName))
						{
							abstractProjectNode.UpdateParseInformation(item[0], item[1]);
						}
						else if (abstractProjectNode.Project == compilationUnit.ProjectContent.Project)
						{
							abstractProjectNode.UpdateParseInformation(item[0], item[1]);
						}
					}
				}
			}
			pending.Clear();
		}
	}

	public void ParserServiceParseInformationUpdated(object sender, ParseInformationEventArgs e)
	{
		lock (pending)
		{
			pending.Add(new ICompilationUnit[2]
			{
				e.ParseInformation.MostRecentCompilationUnit,
				e.CompilationUnit
			});
		}
		WorkbenchSingleton.SafeThreadAsyncCall(UpdateThread);
	}

	public void NavigateBackward()
	{
		if (previousNodes.Count > 0)
		{
			if (classBrowserTreeView.SelectedNode == previousNodes.Peek())
			{
				nextNodes.Push(previousNodes.Pop());
			}
			if (previousNodes.Count > 0)
			{
				navigateBack = true;
				classBrowserTreeView.SelectedNode = previousNodes.Pop();
			}
		}
		UpdateToolbars();
	}

	public void NavigateForward()
	{
		if (nextNodes.Count > 0)
		{
			if (classBrowserTreeView.SelectedNode == nextNodes.Peek())
			{
				previousNodes.Push(nextNodes.Pop());
			}
			if (nextNodes.Count > 0)
			{
				navigateForward = true;
				classBrowserTreeView.SelectedNode = nextNodes.Pop();
			}
		}
		UpdateToolbars();
	}

	private void ClassBrowserTreeViewAfterSelect(object sender, TreeViewEventArgs e)
	{
		if (navigateBack)
		{
			nextNodes.Push(e.Node);
			navigateBack = false;
		}
		else
		{
			if (!navigateForward)
			{
				nextNodes.Clear();
			}
			previousNodes.Push(e.Node);
			navigateForward = false;
		}
		UpdateToolbars();
	}

	public void StartSearch()
	{
		if (searchTerm.Length == 0)
		{
			CancelSearch();
			return;
		}
		if (!inSearchMode)
		{
			foreach (TreeNode node in classBrowserTreeView.Nodes)
			{
				oldNodes.Add(node);
			}
			inSearchMode = true;
			previousNodes.Clear();
			nextNodes.Clear();
			UpdateToolbars();
		}
		classBrowserTreeView.BeginUpdate();
		classBrowserTreeView.Nodes.Clear();
		if (ProjectService.OpenSolution != null)
		{
			foreach (IProject project in ProjectService.OpenSolution.Projects)
			{
				IProjectContent projectContent = ParserService.GetProjectContent(project);
				if (projectContent == null)
				{
					continue;
				}
				foreach (IClass @class in projectContent.Classes)
				{
					if (@class.Name.ToUpper().StartsWith(searchTerm))
					{
						ClassNodeBuilders.AddClassNode(classBrowserTreeView, project, @class);
					}
				}
			}
		}
		if (classBrowserTreeView.Nodes.Count == 0)
		{
			ExtTreeNode extTreeNode = new ExtTreeNode();
			extTreeNode.Text = ResourceService.GetString("MainWindow.Windows.ClassBrowser.NoResultsFound");
			extTreeNode.AddTo(classBrowserTreeView);
		}
		classBrowserTreeView.Sort();
		classBrowserTreeView.EndUpdate();
	}

	public void CancelSearch()
	{
		if (!inSearchMode)
		{
			return;
		}
		classBrowserTreeView.Nodes.Clear();
		foreach (TreeNode oldNode in oldNodes)
		{
			classBrowserTreeView.Nodes.Add(oldNode);
		}
		oldNodes.Clear();
		inSearchMode = false;
		previousNodes.Clear();
		nextNodes.Clear();
		UpdateToolbars();
	}

	private void ProjectServiceSolutionChanged(object sender, EventArgs e)
	{
		classBrowserTreeView.Nodes.Clear();
		foreach (IProject project in ProjectService.OpenSolution.Projects)
		{
			if (!(project is MissingProject) && !(project is UnknownProject))
			{
				ProjectNodeBuilders.AddProjectNode(classBrowserTreeView, project);
			}
		}
	}

	private void ProjectServiceSolutionClosed(object sender, EventArgs e)
	{
		classBrowserTreeView.Nodes.Clear();
		previousNodes.Clear();
		nextNodes.Clear();
		UpdateToolbars();
	}

	private void AmbienceServiceAmbienceChanged(object sender, EventArgs e)
	{
	}

	public void SelectNode(IProject project, string @namespace, string name)
	{
		AbstractProjectNode abstractProjectNode = null;
		foreach (TreeNode node in classBrowserTreeView.Nodes)
		{
			if (node is AbstractProjectNode && project == ((AbstractProjectNode)node).Project)
			{
				abstractProjectNode = (AbstractProjectNode)node;
				break;
			}
		}
		if (abstractProjectNode == null)
		{
			return;
		}
		if (!abstractProjectNode.IsExpanded)
		{
			abstractProjectNode.Expand();
		}
		TreeNode treeNode2 = abstractProjectNode.ExpandNodeByPath(@namespace, create: false);
		if (treeNode2 == null)
		{
			return;
		}
		foreach (TreeNode node2 in treeNode2.Nodes)
		{
			if (!(node2 is ReferenceFolderNode) && node2.Text == name)
			{
				classBrowserTreeView.SelectedNode = node2;
				break;
			}
		}
	}
}
