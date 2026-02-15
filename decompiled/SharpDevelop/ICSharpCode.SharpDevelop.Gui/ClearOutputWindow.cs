using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ClearOutputWindow : AbstractCommand
{
	public override void Run()
	{
		CompilerMessageView.Instance.SelectedMessageViewCategory?.ClearText();
	}
}
