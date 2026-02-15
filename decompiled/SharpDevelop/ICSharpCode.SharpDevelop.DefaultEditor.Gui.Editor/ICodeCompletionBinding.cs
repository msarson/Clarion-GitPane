namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public interface ICodeCompletionBinding
{
	bool HandleKeyPress(SharpDevelopTextAreaControl editor, char ch);
}
