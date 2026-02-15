using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.Commands;

public class GenerateAndMakeSelectedApplicationsInSolutionMenuCommand : ApplicationsInSolutionMenuCommand
{
	protected override void ExecuteApplicationService()
	{
		ApplicationService.GenerateAndMakeApplication(app);
		ProjectBrowserPad.Instance.ProjectBrowserControl.SelectFile(((TreeNode)(object)appNode).Text);
	}
}
