using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class SetAsStartupProject : AbstractMenuCommand
{
	public override void Run()
	{
		AbstractProjectBrowserTreeNode selectedNode = ProjectBrowserPad.Instance.SelectedNode;
		Solution openSolution = ProjectService.OpenSolution;
		if (selectedNode != null && openSolution != null)
		{
			if (selectedNode.Project.IsStartable)
			{
				openSolution.Preferences.StartupProject = selectedNode.Project;
			}
			else
			{
				MessageService.ShowError("${res:BackendBindings.ExecutionManager.CantExecuteDLLError}");
			}
		}
	}
}
