using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class RunProject : AbstractMenuCommand
{
	public override void Run()
	{
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.SelectedNode;
		if (selectedNode != null)
		{
			if (selectedNode.Project.IsStartable)
			{
				selectedNode.Project.Start(withDebugging: true);
			}
			else
			{
				MessageService.ShowError("${res:BackendBindings.ExecutionManager.CantExecuteDLLError}");
			}
		}
	}
}
