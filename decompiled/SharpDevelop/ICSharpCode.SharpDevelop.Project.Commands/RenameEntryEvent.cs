using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class RenameEntryEvent : AbstractMenuCommand
{
	public override void Run()
	{
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.SelectedNode;
		if (selectedNode != null)
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.Select();
			ProjectBrowserPad.Instance.ProjectBrowserControl.Focus();
			ProjectBrowserPad.Instance.StartLabelEdit(selectedNode);
		}
	}
}
