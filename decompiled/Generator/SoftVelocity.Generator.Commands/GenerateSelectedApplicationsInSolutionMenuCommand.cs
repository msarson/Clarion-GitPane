using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.Commands;

public class GenerateSelectedApplicationsInSolutionMenuCommand : ApplicationsInSolutionMenuCommand
{
	protected override void ExecuteApplicationService()
	{
		ApplicationService.GenerationEnded += ApplicationService_GenerationEnded;
		ApplicationService.GenerateApplication(app, GenerationMode.Off, GenerationMode.Off);
	}

	private void ApplicationService_GenerationEnded(object sender, GenerationEndEventArgs e)
	{
		if (appNode != null)
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.SelectFile(((TreeNode)(object)appNode).Text);
		}
	}
}
