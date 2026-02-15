using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ShowClarionColorDialog : AbstractMenuCommand
{
	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null || !(activeWorkbenchWindow.ViewContent is ITextEditorControlProvider))
		{
			return;
		}
		TextEditorControl textEditorControl = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
		using SharpDevelopColorDialog sharpDevelopColorDialog = new SharpDevelopColorDialog();
		if (sharpDevelopColorDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			string selectedClarionColor = sharpDevelopColorDialog.SelectedClarionColor;
			textEditorControl.Document.Insert(textEditorControl.ActiveTextAreaControl.Caret.Offset, selectedClarionColor);
			int lineNumberForOffset = textEditorControl.Document.GetLineNumberForOffset(textEditorControl.ActiveTextAreaControl.Caret.Offset);
			textEditorControl.ActiveTextAreaControl.Caret.Column += selectedClarionColor.Length;
			textEditorControl.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, new TextLocation(0, lineNumberForOffset)));
			textEditorControl.Document.CommitUpdate();
		}
	}
}
