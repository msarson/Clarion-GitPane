using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common.ClarionEditor.Actions;

public class RelativeBlockIndent : AbstractEditAction
{
	private int indent;

	public int Indent
	{
		get
		{
			return indent;
		}
		set
		{
			indent = value;
		}
	}

	public override void Execute(TextArea textArea)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (indent == 0)
		{
			return;
		}
		textArea.Document.UndoStack.StartUndoGroup();
		textArea.BeginUpdate();
		try
		{
			int tabIndent = textArea.Document.TextEditorProperties.TabIndent;
			foreach (ISelection item in textArea.SelectionManager.SelectionCollection)
			{
				TextLocation endPosition = item.EndPosition;
				int num = ((TextLocation)(ref endPosition)).Y;
				while (true)
				{
					int num2 = num;
					TextLocation startPosition = item.StartPosition;
					if (num2 < ((TextLocation)(ref startPosition)).Y)
					{
						break;
					}
					int num3 = num;
					TextLocation endPosition2 = item.EndPosition;
					if (num3 == ((TextLocation)(ref endPosition2)).Y)
					{
						TextLocation endPosition3 = item.EndPosition;
						if (((TextLocation)(ref endPosition3)).X == 0)
						{
							goto IL_0111;
						}
					}
					LineSegment lineSegment = textArea.Document.GetLineSegment(num);
					string text = textArea.Document.GetText((ISegment)(object)lineSegment);
					if (!(text == string.Empty))
					{
						int currentIndentSize = GetCurrentIndentSize(text, tabIndent);
						currentIndentSize += indent;
						if (currentIndentSize < 0)
						{
							currentIndentSize = 0;
						}
						textArea.Document.Replace(lineSegment.Offset, lineSegment.Length, CreateNewIndent(currentIndentSize, tabIndent, textArea.Document.TextEditorProperties.ConvertTabsToSpaces) + text.TrimStart());
					}
					goto IL_0111;
					IL_0111:
					num--;
				}
				IDocument document = textArea.Document;
				TextLocation startPosition2 = item.StartPosition;
				int y = ((TextLocation)(ref startPosition2)).Y;
				TextLocation endPosition4 = item.EndPosition;
				document.RequestUpdate(new TextAreaUpdate((TextAreaUpdateType)5, y, ((TextLocation)(ref endPosition4)).Y));
			}
			textArea.Document.CommitUpdate();
			textArea.AutoClearSelection = false;
		}
		finally
		{
			textArea.EndUpdate();
			textArea.Document.UndoStack.EndUndoGroup();
		}
	}

	private static string CreateNewIndent(int indentSize, int tabSize, bool convertTabsToSpaces)
	{
		if (convertTabsToSpaces)
		{
			return new string(' ', indentSize);
		}
		int count = indentSize / tabSize;
		int count2 = indentSize % tabSize;
		return new string('\t', count) + new string(' ', count2);
	}

	private static int GetCurrentIndentSize(string lineText, int tabSize)
	{
		int num = 0;
		for (int i = 0; i < lineText.Length; i++)
		{
			if (lineText[i] == ' ')
			{
				num++;
				continue;
			}
			if (lineText[i] != '\t')
			{
				break;
			}
			num += tabSize;
		}
		return num;
	}
}
