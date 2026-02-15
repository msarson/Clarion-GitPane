using System;
using System.Collections;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class CommentCompletionDataProvider : AbstractCompletionDataProvider
{
	private class CommentCompletionData : ICompletionData, IComparable
	{
		private string text;

		private string description;

		public int ImageIndex => 34;

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
			}
		}

		public string Description => description;

		public double Priority => 0.0;

		public bool InsertAction(TextArea textArea, char ch)
		{
			textArea.InsertString(text);
			return false;
		}

		public CommentCompletionData(string text, string description)
		{
			this.text = text;
			this.description = description;
		}

		public int CompareTo(object obj)
		{
			if (obj == null || !(obj is CommentCompletionData))
			{
				return -1;
			}
			return text.CompareTo(((CommentCompletionData)obj).text);
		}
	}

	private int caretLineNumber;

	private int caretColumn;

	private string[][] commentTags = new string[20][]
	{
		new string[2] { "c", "marks text as code" },
		new string[2] { "code", "marks text as code" },
		new string[2] { "example", "A description of the code example\n(must have a <code> tag inside)" },
		new string[2] { "exception cref=\"\"", "description to an exception thrown" },
		new string[2] { "list type=\"\"", "A list" },
		new string[2] { "listheader", "The header from the list" },
		new string[2] { "item", "A list item" },
		new string[2] { "term", "A term in a list" },
		new string[2] { "description", "A description to a term in a list" },
		new string[2] { "para", "A text paragraph" },
		new string[2] { "param name=\"\"", "A description for a parameter" },
		new string[2] { "paramref name=\"\"", "A reference to a parameter" },
		new string[2] { "permission cref=\"\"", "" },
		new string[2] { "remarks", "Gives description for a member" },
		new string[2] { "include file=\"\" path=\"\"", "Includes comments from other files" },
		new string[2] { "returns", "Gives description for a return value" },
		new string[2] { "see cref=\"\"", "A reference to a member" },
		new string[2] { "seealso cref=\"\"", "A reference to a member in the seealso section" },
		new string[2] { "summary", "A summary of the object" },
		new string[2] { "value", "A description of a property" }
	};

	private bool IsBetween(int row, int column, DomRegion region)
	{
		if (row >= region.BeginLine)
		{
			if (row > region.EndLine)
			{
				return region.EndLine == -1;
			}
			return true;
		}
		return false;
	}

	public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
	{
		caretLineNumber = textArea.Caret.Line;
		caretColumn = textArea.Caret.Column;
		LineSegment lineSegment = textArea.Document.GetLineSegment(caretLineNumber);
		string text = textArea.Document.GetText(lineSegment.Offset, lineSegment.Length);
		if (!text.Trim().StartsWith("///") && !text.Trim().StartsWith("'''"))
		{
			return null;
		}
		ArrayList arrayList = new ArrayList();
		string[][] array = commentTags;
		foreach (string[] array2 in array)
		{
			arrayList.Add(new CommentCompletionData(array2[0], array2[1]));
		}
		return (ICompletionData[])arrayList.ToArray(typeof(ICompletionData));
	}
}
