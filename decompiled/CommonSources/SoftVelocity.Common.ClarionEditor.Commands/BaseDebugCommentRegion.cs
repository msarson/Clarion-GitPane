using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public abstract class BaseDebugCommentRegion : AbstractMenuCommand
{
	private int firstLine;

	private int lastLine;

	protected abstract bool AddComment { get; }

	public override void Run()
	{
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null || !(activeWorkbenchWindow.ActiveViewContent is CommonClarionEditor commonClarionEditor))
		{
			return;
		}
		TextArea textArea = ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).TextEditorControl).ActiveTextAreaControl.TextArea;
		if (textArea.SelectionManager.HasSomethingSelected)
		{
			if (textArea.SelectionManager.SelectionIsReadonly)
			{
				return;
			}
			foreach (ISelection item in textArea.SelectionManager.SelectionCollection)
			{
				textArea.BeginUpdate();
				textArea.Document.UndoStack.StartUndoGroup();
				if (AddComment)
				{
					IDocument document = textArea.Document;
					TextLocation startPosition = item.StartPosition;
					int y = ((TextLocation)(ref startPosition)).Y;
					TextLocation endPosition = item.EndPosition;
					SetDebugCommentAt(document, item, y, ((TextLocation)(ref endPosition)).Y);
				}
				else
				{
					IDocument document2 = textArea.Document;
					TextLocation startPosition2 = item.StartPosition;
					int y2 = ((TextLocation)(ref startPosition2)).Y;
					TextLocation endPosition2 = item.EndPosition;
					RemoveDebugCommentAt(document2, item, y2, ((TextLocation)(ref endPosition2)).Y);
				}
				textArea.Document.UndoStack.EndUndoGroup();
				textArea.Document.UpdateQueue.Clear();
				textArea.Document.RequestUpdate(new TextAreaUpdate((TextAreaUpdateType)5, firstLine, lastLine));
				textArea.EndUpdate();
			}
			textArea.Document.CommitUpdate();
			textArea.AutoClearSelection = false;
			return;
		}
		int line = textArea.Caret.Line;
		if (!textArea.IsReadOnly(line))
		{
			textArea.BeginUpdate();
			textArea.Document.UndoStack.StartUndoGroup();
			if (AddComment)
			{
				SetDebugCommentAt(textArea.Document, null, line, line);
			}
			else
			{
				RemoveDebugCommentAt(textArea.Document, null, line, line);
			}
			textArea.Document.UndoStack.EndUndoGroup();
			textArea.Document.UpdateQueue.Clear();
			textArea.Document.RequestUpdate(new TextAreaUpdate((TextAreaUpdateType)1, line));
			textArea.EndUpdate();
		}
	}

	private void SetDebugCommentAt(IDocument document, ISelection selection, int y1, int y2)
	{
		firstLine = y1;
		lastLine = y2;
		for (int num = y2; num >= y1; num--)
		{
			LineSegment lineSegment = document.GetLineSegment(num);
			if (selection != null && num == y2 && lineSegment.Offset == selection.Offset + selection.Length)
			{
				lastLine--;
			}
			else
			{
				string text = document.GetText(lineSegment.Offset, lineSegment.Length);
				if (!(text.TrimStart() == string.Empty) && text[0] != '?')
				{
					document.Insert(lineSegment.Offset, "?");
				}
			}
		}
	}

	private void RemoveDebugCommentAt(IDocument document, ISelection selection, int y1, int y2)
	{
		firstLine = y1;
		lastLine = y2;
		for (int num = y2; num >= y1; num--)
		{
			LineSegment lineSegment = document.GetLineSegment(num);
			if (selection != null && num == y2 && lineSegment.Offset == selection.Offset + selection.Length)
			{
				lastLine--;
			}
			else
			{
				string text = document.GetText(lineSegment.Offset, lineSegment.Length);
				if (!(text == string.Empty) && text[0] == '?')
				{
					document.Remove(lineSegment.Offset, 1);
				}
			}
		}
	}
}
