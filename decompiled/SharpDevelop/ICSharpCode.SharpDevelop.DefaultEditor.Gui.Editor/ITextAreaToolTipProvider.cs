using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public interface ITextAreaToolTipProvider
{
	ToolTipInfo GetToolTipInfo(TextArea textArea, ToolTipRequestEventArgs e);
}
