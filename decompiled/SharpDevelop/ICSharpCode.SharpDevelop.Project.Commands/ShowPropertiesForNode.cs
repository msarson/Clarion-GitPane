using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ShowPropertiesForNode : AbstractMenuCommand
{
	public override void Run()
	{
		ProjectBrowserPad.Instance.SelectedNode?.ShowProperties();
	}
}
