using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class CloseSolution : AbstractMenuCommand
{
	public override void Run()
	{
		ProjectService.SaveSolutionPreferences();
		if (WorkbenchSingleton.Workbench.CloseAllSolutionViews())
		{
			ProjectService.CloseSolution();
		}
	}
}
