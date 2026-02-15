using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.Dialogs;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public abstract class BaseOCCommentRegion : AbstractMenuCommand
{
	private int firstLine;

	private int lastLine;

	protected abstract string Command { get; }

	public override void Run()
	{
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
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
		}
		else if (textArea.IsReadOnly(textArea.Caret.Line))
		{
			return;
		}
		string terminator;
		string expression;
		using (NewOmitCompileDialog newOmitCompileDialog = new NewOmitCompileDialog(Command))
		{
			if (newOmitCompileDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			terminator = newOmitCompileDialog.Terminator;
			expression = newOmitCompileDialog.Expression;
		}
		if (textArea.SelectionManager.HasSomethingSelected)
		{
			foreach (ISelection item in textArea.SelectionManager.SelectionCollection)
			{
				textArea.BeginUpdate();
				textArea.Document.UndoStack.StartUndoGroup();
				TextLocation startPosition = item.StartPosition;
				int y = ((TextLocation)(ref startPosition)).Y;
				TextLocation endPosition = item.EndPosition;
				AddCommand(textArea, terminator, expression, item, y, ((TextLocation)(ref endPosition)).Y);
				textArea.Document.UndoStack.EndUndoGroup();
				textArea.Document.UpdateQueue.Clear();
				textArea.Document.RequestUpdate(new TextAreaUpdate((TextAreaUpdateType)4, new TextLocation(0, firstLine - 1)));
				textArea.EndUpdate();
			}
			textArea.Document.CommitUpdate();
			textArea.SelectionManager.ClearSelection();
		}
		else
		{
			textArea.BeginUpdate();
			int line = textArea.Caret.Line;
			textArea.Document.UndoStack.StartUndoGroup();
			AddCommand(textArea, terminator, expression, null, line, line);
			textArea.Document.UndoStack.EndUndoGroup();
			textArea.Document.UpdateQueue.Clear();
			textArea.Document.RequestUpdate(new TextAreaUpdate((TextAreaUpdateType)4, new TextLocation(0, firstLine - 1)));
			textArea.EndUpdate();
		}
	}

	private void AddCommand(TextArea textArea, string terminator, string expression, ISelection selection, int y1, int y2)
	{
		firstLine = y1;
		lastLine = y2;
		string text = Command + "('" + terminator + "'";
		if (expression.Length != 0)
		{
			text = text + ", " + expression;
		}
		text += ")";
		string indentString = Tab.GetIndentationString(textArea.Document);
		LineSegment lineSegment = textArea.Document.GetLineSegment(lastLine);
		if (selection != null && lineSegment.Offset == selection.Offset + selection.Length)
		{
			lastLine--;
		}
		InsertCommand(textArea, text, firstLine, ref indentString);
		lastLine++;
		InsertCommandEnd(textArea, text, lastLine, indentString);
	}

	private static int InsertCommandEnd(TextArea textArea, string commandText, int lastLineNum, string indentString)
	{
		IDocument document = textArea.Document;
		LineSegment lineSegment = document.GetLineSegment(lastLineNum);
		string lineSeparator = GetLineSeparator(document);
		document.Insert(lineSegment.Offset + lineSegment.Length, lineSeparator + indentString + "!end of " + commandText);
		return 1;
	}

	private static int InsertCommand(TextArea textArea, string commandText, int lineNum, ref string indentString)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		IDocument document = textArea.Document;
		LineSegment lineSegment = document.GetLineSegment(lineNum);
		string lineSeparator = GetLineSeparator(document);
		if ((int)document.TextEditorProperties.IndentStyle == 2)
		{
			document.Insert(lineSegment.Offset, commandText + lineSeparator);
			document.FormattingStrategy.IndentLine(textArea, lineNum);
			string text = document.GetText((ISegment)(object)document.GetLineSegment(lineNum));
			string text2 = text.Substring(0, text.Length - text.TrimStart().Length);
			if (text2.Length != 0)
			{
				indentString = text2;
			}
			return 2;
		}
		string text3 = document.GetText(lineSegment.Offset, lineSegment.Length);
		string text4 = text3.Substring(0, text3.Length - text3.TrimStart().Length);
		if (text4.Length == 0)
		{
			text4 = indentString;
		}
		document.Insert(lineSegment.Offset, text4 + commandText + lineSeparator);
		return 1;
	}

	private static string GetLineSeparator(IDocument document)
	{
		string result = "\r\n";
		if (document.TotalNumberOfLines > 0)
		{
			LineSegment lineSegment = document.GetLineSegment(0);
			if (lineSegment.Length < lineSegment.TotalLength)
			{
				result = document.GetText(lineSegment.Offset + lineSegment.Length, lineSegment.TotalLength - lineSegment.Length);
			}
		}
		return result;
	}
}
