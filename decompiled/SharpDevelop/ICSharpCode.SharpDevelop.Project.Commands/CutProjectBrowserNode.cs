using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class CutProjectBrowserNode : AbstractMenuCommand
{
	public override bool IsEnabled => ProjectBrowserPad.Instance.EnableCut;

	public override void Run()
	{
		ProjectBrowserPad.Instance.Cut();
	}
}
