using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class InsertGuidCommand : AbstractMenuCommand
{
	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ViewContent is ITextEditorControlProvider)
		{
			TextEditorControl textEditorControl = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
			if (textEditorControl != null)
			{
				string str = Guid.NewGuid().ToString().ToUpperInvariant();
				textEditorControl.ActiveTextAreaControl.TextArea.InsertString(str);
			}
		}
	}
}
