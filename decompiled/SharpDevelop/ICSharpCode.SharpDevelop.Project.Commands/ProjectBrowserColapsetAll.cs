using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ProjectBrowserColapsetAll : AbstractMenuCommand
{
	public override void Run()
	{
		ProjectBrowserPad.Instance.ProjectBrowserControl.CollapseAll();
	}
}
