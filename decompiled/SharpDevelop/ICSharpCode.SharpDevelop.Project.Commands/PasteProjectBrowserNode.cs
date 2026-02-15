using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class PasteProjectBrowserNode : AbstractMenuCommand
{
	public override bool IsEnabled => ProjectBrowserPad.Instance.EnablePaste;

	public override void Run()
	{
		ProjectBrowserPad.Instance.Paste();
	}
}
