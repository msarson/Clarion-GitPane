using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ShowColorDialog : AbstractMenuCommand
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
			string text;
			switch (Path.GetExtension(textEditorControl.FileName).ToLowerInvariant())
			{
			case ".cs":
			case ".vb":
			case ".boo":
				text = ((!sharpDevelopColorDialog.Color.IsKnownColor) ? ((sharpDevelopColorDialog.Color.A >= byte.MaxValue) ? $"Color.FromArgb({sharpDevelopColorDialog.Color.R}, {sharpDevelopColorDialog.Color.G}, {sharpDevelopColorDialog.Color.B})" : ("Color.FromArgb(0x" + sharpDevelopColorDialog.Color.ToArgb().ToString("x") + ")")) : ("Color." + sharpDevelopColorDialog.Color.ToKnownColor()));
				break;
			default:
				text = ((!sharpDevelopColorDialog.Color.IsKnownColor) ? ((sharpDevelopColorDialog.Color.A >= byte.MaxValue) ? $"#{sharpDevelopColorDialog.Color.R:X2}{sharpDevelopColorDialog.Color.G:X2}{sharpDevelopColorDialog.Color.B:X2}" : ("#" + sharpDevelopColorDialog.Color.ToArgb().ToString("X"))) : sharpDevelopColorDialog.Color.ToKnownColor().ToString());
				break;
			}
			textEditorControl.Document.Insert(textEditorControl.ActiveTextAreaControl.Caret.Offset, text);
			int lineNumberForOffset = textEditorControl.Document.GetLineNumberForOffset(textEditorControl.ActiveTextAreaControl.Caret.Offset);
			textEditorControl.ActiveTextAreaControl.Caret.Column += text.Length;
			textEditorControl.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, new TextLocation(0, lineNumberForOffset)));
			textEditorControl.Document.CommitUpdate();
		}
	}
}
