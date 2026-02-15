using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class DeleteProjectBrowserNode : AbstractMenuCommand
{
	public override bool IsEnabled => ProjectBrowserPad.Instance.EnableDelete;

	public override void Run()
	{
		ProjectBrowserPad.Instance.Delete();
	}
}
