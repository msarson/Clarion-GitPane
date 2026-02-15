using System.Collections;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class SortSelection : AbstractMenuCommand
{
	internal enum SortDirection
	{
		Ascending,
		Descending
	}

	private class SortComparer : IComparer
	{
		private SortDirection sortDirection;

		private bool isCaseSensitive;

		private bool ignoreWhitespaces;

		public SortComparer()
		{
			isCaseSensitive = PropertyService.Get(SortOptionsDialog.caseSensitiveOption, defaultValue: true);
			ignoreWhitespaces = PropertyService.Get(SortOptionsDialog.ignoreWhiteSpacesOption, defaultValue: true);
			sortDirection = PropertyService.Get(SortOptionsDialog.sortDirectionOption, SortDirection.Ascending);
		}

		public int Compare(object x, object y)
		{
			if (x == null || y == null)
			{
				return -1;
			}
			string text;
			string text2;
			if (sortDirection == SortDirection.Ascending)
			{
				text = x.ToString();
				text2 = y.ToString();
			}
			else
			{
				text = y.ToString();
				text2 = x.ToString();
			}
			if (ignoreWhitespaces)
			{
				text = text.Trim();
				text2 = text2.Trim();
			}
			if (!isCaseSensitive)
			{
				text = text.ToUpper();
				text2 = text2.ToUpper();
			}
			return text.CompareTo(text2);
		}
	}

	public void SortLines(IDocument document, int startLine, int endLine)
	{
		ArrayList arrayList = new ArrayList();
		for (int i = startLine; i <= endLine; i++)
		{
			LineSegment lineSegment = document.GetLineSegment(i);
			arrayList.Add(document.GetText(lineSegment.Offset, lineSegment.Length));
		}
		arrayList.Sort(new SortComparer());
		if (PropertyService.Get(SortOptionsDialog.removeDupesOption, defaultValue: false))
		{
			for (int j = 0; j < arrayList.Count - 1; j++)
			{
				if (arrayList[j].Equals(arrayList[j + 1]))
				{
					arrayList.RemoveAt(j);
					j--;
				}
			}
		}
		for (int k = 0; k < arrayList.Count; k++)
		{
			LineSegment lineSegment2 = document.GetLineSegment(startLine + k);
			document.Replace(lineSegment2.Offset, lineSegment2.Length, arrayList[k].ToString());
		}
		for (int l = startLine + arrayList.Count; l <= endLine; l++)
		{
			LineSegment lineSegment3 = document.GetLineSegment(startLine + arrayList.Count);
			document.Remove(lineSegment3.Offset, lineSegment3.TotalLength);
		}
	}

	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null || !(activeWorkbenchWindow.ViewContent is ITextEditorControlProvider))
		{
			return;
		}
		using SortOptionsDialog sortOptionsDialog = new SortOptionsDialog();
		sortOptionsDialog.Owner = (Form)WorkbenchSingleton.Workbench;
		if (sortOptionsDialog.ShowDialog(WorkbenchSingleton.MainForm) != DialogResult.OK)
		{
			return;
		}
		TextArea textArea = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl.ActiveTextAreaControl.TextArea;
		textArea.BeginUpdate();
		if (textArea.SelectionManager.HasSomethingSelected)
		{
			foreach (ISelection item in textArea.SelectionManager.SelectionCollection)
			{
				SortLines(textArea.Document, item.StartPosition.Y, item.EndPosition.Y);
			}
		}
		else
		{
			SortLines(textArea.Document, 0, textArea.Document.TotalNumberOfLines - 1);
		}
		textArea.Caret.ValidateCaretPos();
		textArea.EndUpdate();
		textArea.Refresh();
	}
}
