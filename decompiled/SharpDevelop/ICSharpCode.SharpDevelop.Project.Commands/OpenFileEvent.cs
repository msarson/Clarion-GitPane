using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class OpenFileEvent : AbstractMenuCommand
{
	public override void Run()
	{
		ProjectBrowserPad.Instance.SelectedNode?.ActivateItem();
	}
}
