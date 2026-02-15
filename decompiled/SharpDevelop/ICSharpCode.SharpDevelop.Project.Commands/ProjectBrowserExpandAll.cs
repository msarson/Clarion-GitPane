using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ProjectBrowserExpandAll : AbstractMenuCommand
{
	public override void Run()
	{
		ProjectBrowserPad.Instance.ProjectBrowserControl.ExpandAll();
	}
}
