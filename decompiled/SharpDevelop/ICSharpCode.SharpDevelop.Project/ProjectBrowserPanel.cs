using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class ProjectBrowserPanel : UserControl
{
	private ToolStrip toolStrip;

	private ProjectBrowserControl projectBrowserControl;

	private ToolStripItem[] standardItems;

	public AbstractProjectBrowserTreeNode SelectedNode => projectBrowserControl.SelectedNode;

	public AbstractProjectBrowserTreeNode RootNode => projectBrowserControl.RootNode;

	public ProjectBrowserControl ProjectBrowserControl => projectBrowserControl;

	public ProjectBrowserPanel()
	{
		projectBrowserControl = new ProjectBrowserControl();
		projectBrowserControl.Dock = DockStyle.Fill;
		base.Controls.Add(projectBrowserControl);
		toolStrip = ToolbarService.CreateToolStrip(this, "/SharpDevelop/Pads/ProjectBrowser/ToolBar/Standard");
		toolStrip.ShowItemToolTips = true;
		toolStrip.Dock = DockStyle.Top;
		toolStrip.GripStyle = ToolStripGripStyle.Hidden;
		toolStrip.Stretch = true;
		standardItems = new ToolStripItem[toolStrip.Items.Count];
		toolStrip.Items.CopyTo(standardItems, 0);
		base.Controls.Add(toolStrip);
		projectBrowserControl.TreeView.BeforeSelect += TreeViewBeforeSelect;
	}

	private void TreeViewBeforeSelect(object sender, TreeViewCancelEventArgs e)
	{
		UpdateToolStrip(e.Node as AbstractProjectBrowserTreeNode);
	}

	private void UpdateToolStrip(AbstractProjectBrowserTreeNode node)
	{
		toolStrip.Items.Clear();
		toolStrip.Items.AddRange(standardItems);
		ToolbarService.UpdateToolbar(toolStrip);
		if (node != null && node.ToolbarAddinTreePath != null)
		{
			toolStrip.Items.Add(new ToolStripSeparator());
			toolStrip.Items.AddRange((ToolStripItem[])AddInTree.BuildItems(node.ToolbarAddinTreePath, node, throwOnNotFound: false).ToArray(typeof(ToolStripItem)));
		}
	}

	public void ViewSolution(Solution solution)
	{
		UpdateToolStrip(null);
		projectBrowserControl.ViewSolution(solution);
	}

	public void StoreViewState(Properties memento)
	{
		projectBrowserControl.StoreViewState(memento);
		memento.Set("ProjectBrowserState", ExtTreeView.GetViewStateString(projectBrowserControl.TreeView));
	}

	public void ReadViewState(Properties memento)
	{
		projectBrowserControl.ReadViewState(memento);
		ExtTreeView.ApplyViewStateString(memento.Get("ProjectBrowserState", ""), projectBrowserControl.TreeView);
	}

	public void Clear()
	{
		projectBrowserControl.Clear();
		UpdateToolStrip(null);
	}

	public void SelectFile(string fileName)
	{
		projectBrowserControl.SelectFile(fileName);
	}
}
