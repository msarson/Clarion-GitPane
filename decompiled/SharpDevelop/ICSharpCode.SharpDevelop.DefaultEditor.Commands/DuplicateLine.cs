using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class DuplicateLine : AbstractMenuCommand
{
	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ViewContent is ITextEditorControlProvider)
		{
			TextEditorControl textEditorControl = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
			TextArea textArea = textEditorControl.ActiveTextAreaControl.TextArea;
			IDocument document = textArea.Document;
			if (document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal && textArea.SelectionManager.SelectionCollection.Count > 0)
			{
				textArea.SelectionManager.ClearSelection();
			}
			int line = textArea.Caret.Line;
			if (!textArea.IsReadOnly(line))
			{
				LineSegment lineSegment = document.GetLineSegment(line);
				string text = Environment.NewLine + document.GetText(lineSegment.Offset, lineSegment.Length);
				textArea.BeginUpdate();
				document.Insert(lineSegment.Offset + lineSegment.Length, text);
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, line, line));
				textArea.EndUpdate();
			}
		}
	}
}
