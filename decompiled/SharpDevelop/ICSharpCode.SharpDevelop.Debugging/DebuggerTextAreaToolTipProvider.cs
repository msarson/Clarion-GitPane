using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.Debugging;

public class DebuggerTextAreaToolTipProvider : ITextAreaToolTipProvider
{
	public ToolTipInfo GetToolTipInfo(TextArea textArea, ToolTipRequestEventArgs e)
	{
		return DebuggerService.GetToolTipInfo(textArea, e);
	}
}
