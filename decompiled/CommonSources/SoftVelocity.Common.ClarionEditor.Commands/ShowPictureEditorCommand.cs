using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using SoftVelocity.ClarionNet.Windows;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class ShowPictureEditorCommand : AbstractMenuCommand
{
	public override void Run()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ViewContent is ITextEditorControlProvider)
		{
			TextEditorControl textEditorControl = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
			string editedPicture = string.Empty;
			if (FormatPicture.Edit("", ref editedPicture))
			{
				((TextEditorControlBase)textEditorControl).Document.Insert(((TextEditorControlBase)textEditorControl).ActiveTextAreaControl.Caret.Offset, editedPicture);
				int lineNumberForOffset = ((TextEditorControlBase)textEditorControl).Document.GetLineNumberForOffset(((TextEditorControlBase)textEditorControl).ActiveTextAreaControl.Caret.Offset);
				Caret caret = ((TextEditorControlBase)textEditorControl).ActiveTextAreaControl.Caret;
				caret.Column += editedPicture.Length;
				((TextEditorControlBase)textEditorControl).Document.RequestUpdate(new TextAreaUpdate((TextAreaUpdateType)1, new TextLocation(0, lineNumberForOffset)));
				((TextEditorControlBase)textEditorControl).Document.CommitUpdate();
			}
		}
	}
}
