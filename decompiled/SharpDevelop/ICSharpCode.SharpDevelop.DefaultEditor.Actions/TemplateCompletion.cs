using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Actions;

public class TemplateCompletion : AbstractEditAction
{
	public override void Execute(TextArea services)
	{
		SharpDevelopTextAreaControl sharpDevelopTextAreaControl = (SharpDevelopTextAreaControl)services.MotherTextEditorControl;
		services.AutoClearSelection = false;
		sharpDevelopTextAreaControl.ShowCompletionWindow(new TemplateCompletionDataProvider(), '\0');
	}
}
