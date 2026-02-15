using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class CopyProjectBrowserNode : AbstractMenuCommand
{
	public override bool IsEnabled => ProjectBrowserPad.Instance.EnableCopy;

	public override void Run()
	{
		ProjectBrowserPad.Instance.Copy();
	}
}
