using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.Commands;

public class OpenApplicationsDictionaryInSolutionMenuCommand : ApplicationsInSolutionMenuCommand
{
	protected override void ExecuteApplicationService()
	{
		ApplicationService.OpenDictionary(app);
		ProjectBrowserPad.Instance.ProjectBrowserControl.SelectFile(((TreeNode)(object)appNode).Text);
	}
}
