using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class EditConfigurationsCommand : AbstractMenuCommand
{
	public override void Run()
	{
		using SolutionConfigurationEditor solutionConfigurationEditor = new SolutionConfigurationEditor();
		solutionConfigurationEditor.ShowDialog();
		ProjectService.SaveSolution();
		ProjectService.OpenSolution.ApplySolutionConfigurationAndPlatformToProjects();
		ProjectBrowserPad.Instance.ProjectBrowserControl.RefreshView();
	}
}
