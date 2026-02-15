using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class ProjectBrowserPad : AbstractPadContent, IClipboardHandler, IHasPropertyContainer
{
	private static ProjectBrowserPad instance;

	private ProjectBrowserPanel projectBrowserPanel = new ProjectBrowserPanel();

	private string lastFileName;

	public static ProjectBrowserPad Instance
	{
		get
		{
			if (instance == null)
			{
				PadDescriptor padDescriptor = ((WorkbenchSingleton.Workbench == null) ? null : WorkbenchSingleton.Workbench.GetPad(typeof(ProjectBrowserPad)));
				if (padDescriptor != null)
				{
					padDescriptor.CreatePad();
				}
				else
				{
					instance = new ProjectBrowserPad();
				}
			}
			return instance;
		}
	}

	public AbstractProjectBrowserTreeNode SelectedNode => projectBrowserPanel.SelectedNode;

	public ProjectNode CurrentProject
	{
		get
		{
			AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode = SelectedNode;
			while (abstractProjectBrowserTreeNode != null && !(abstractProjectBrowserTreeNode is ProjectNode))
			{
				abstractProjectBrowserTreeNode = (AbstractProjectBrowserTreeNode)abstractProjectBrowserTreeNode.Parent;
			}
			return (ProjectNode)abstractProjectBrowserTreeNode;
		}
	}

	public AbstractProjectBrowserTreeNode SolutionNode => projectBrowserPanel.RootNode;

	public ProjectBrowserControl ProjectBrowserControl => projectBrowserPanel.ProjectBrowserControl;

	public override Control Control => projectBrowserPanel;

	public PropertyContainer PropertyContainer => projectBrowserPanel.ProjectBrowserControl.PropertyContainer;

	public bool EnableCut
	{
		get
		{
			if (!(ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode))
			{
				return false;
			}
			return extTreeNode.EnableCut;
		}
	}

	public bool EnableCopy
	{
		get
		{
			if (!(ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode))
			{
				return false;
			}
			return extTreeNode.EnableCopy;
		}
	}

	public bool EnablePaste
	{
		get
		{
			if (!(ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode))
			{
				return false;
			}
			return extTreeNode.EnablePaste;
		}
	}

	public bool EnableDelete
	{
		get
		{
			if (!(ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode))
			{
				return false;
			}
			return extTreeNode.EnableDelete;
		}
	}

	public bool EnableSelectAll
	{
		get
		{
			if (!(ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode))
			{
				return false;
			}
			return extTreeNode.EnableSelectAll;
		}
	}

	public ProjectBrowserPad()
	{
		instance = this;
		ProjectService.SolutionLoadedFirstChanceCall += ProjectServiceSolutionLoaded;
		ProjectService.SolutionClosed += ProjectServiceSolutionClosed;
		ProjectService.SolutionPreferencesSaving += ProjectServiceSolutionPreferencesSaving;
		if (WorkbenchSingleton.Workbench != null)
		{
			WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += ActiveWindowChanged;
		}
		if (ProjectService.OpenSolution != null)
		{
			ProjectServiceSolutionLoaded(null, new SolutionEventArgs(ProjectService.OpenSolution));
		}
		if (WorkbenchSingleton.Workbench != null)
		{
			ActiveWindowChanged(null, null);
		}
	}

	public void StartLabelEdit(ExtTreeNode node)
	{
		ProjectBrowserControl.TreeView.StartLabelEdit(node);
	}

	private void ProjectServiceSolutionPreferencesSaving(object sender, SolutionEventArgs e)
	{
		projectBrowserPanel.StoreViewState(e.Solution.Preferences.Properties);
	}

	private void DisplaySolution(Solution solution)
	{
		projectBrowserPanel.ViewSolution(solution);
		projectBrowserPanel.ReadViewState(solution.Preferences.Properties);
	}

	private void ProjectServiceSolutionLoaded(object sender, SolutionEventArgs e)
	{
		DisplaySolution(e.Solution);
	}

	public void RefreshSolution()
	{
		DisplaySolution(SolutionNode.Solution);
	}

	private void ProjectServiceSolutionClosed(object sender, EventArgs e)
	{
		projectBrowserPanel.Clear();
	}

	private void ActiveWindowChanged(object sender, EventArgs e)
	{
		if (WorkbenchSingleton.Workbench.ActiveContent == this)
		{
			projectBrowserPanel.ProjectBrowserControl.PadActivated();
			return;
		}
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null)
		{
			string fileName = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName;
			if (fileName != null && !(lastFileName == fileName) && FileUtility.IsValidFileName(fileName))
			{
				lastFileName = fileName;
				projectBrowserPanel.SelectFile(fileName);
			}
		}
	}

	public void Cut()
	{
		ProjectBrowserControl.TreeView.ClearCutNodes();
		if (ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode)
		{
			extTreeNode.Cut();
		}
	}

	public void Copy()
	{
		ProjectBrowserControl.TreeView.ClearCutNodes();
		if (ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode)
		{
			extTreeNode.Copy();
		}
	}

	public void Paste()
	{
		if (ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode)
		{
			extTreeNode.Paste();
		}
		ProjectBrowserControl.TreeView.ClearCutNodes();
	}

	public void Delete()
	{
		TreeNode treeNode = null;
		if (ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode)
		{
			if (extTreeNode != null)
			{
				treeNode = extTreeNode.PrevNode;
				if (treeNode != null && treeNode.Parent != extTreeNode.Parent)
				{
					treeNode = null;
				}
			}
			extTreeNode.Delete();
		}
		ProjectBrowserControl.TreeView.ClearCutNodes();
		if (treeNode != null)
		{
			TreeNode[] array = ProjectBrowserControl.TreeView.Nodes.Find(treeNode.Text, searchAllChildren: true);
			if (array.Length > 0)
			{
				ProjectBrowserControl.TreeView.SelectedNode = array[0];
			}
		}
	}

	public void SelectAll()
	{
		if (ProjectBrowserControl.TreeView.SelectedNode is ExtTreeNode extTreeNode)
		{
			extTreeNode.SelectAll();
		}
	}
}
