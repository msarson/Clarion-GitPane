using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Actions;

public class CodeCompletionPopup : AbstractEditAction
{
	public override void Execute(TextArea services)
	{
		SharpDevelopTextAreaControl sharpDevelopTextAreaControl = (SharpDevelopTextAreaControl)services.MotherTextEditorControl;
		if (CodeCompletionOptions.EnableCodeCompletion)
		{
			sharpDevelopTextAreaControl.ShowCompletionWindow(new CtrlSpaceCompletionDataProvider(), '\0');
		}
	}
}
