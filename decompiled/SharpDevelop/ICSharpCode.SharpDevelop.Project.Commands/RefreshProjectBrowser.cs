using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class RefreshProjectBrowser : AbstractMenuCommand
{
	public override void Run()
	{
		ProjectBrowserPad.Instance.ProjectBrowserControl.RefreshView();
	}
}
