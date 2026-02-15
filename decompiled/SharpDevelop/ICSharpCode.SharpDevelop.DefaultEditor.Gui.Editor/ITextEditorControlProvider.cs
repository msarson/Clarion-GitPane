using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public interface ITextEditorControlProvider
{
	TextEditorControl TextEditorControl { get; }
}
