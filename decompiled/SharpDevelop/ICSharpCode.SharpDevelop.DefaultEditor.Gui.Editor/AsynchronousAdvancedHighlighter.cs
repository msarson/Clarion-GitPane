using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public abstract class AsynchronousAdvancedHighlighter : IAdvancedHighlighter, IDisposable
{
	private readonly object lockObject = new object();

	private Dictionary<LineSegment, List<TextWord>> outstanding = new Dictionary<LineSegment, List<TextWord>>();

	private TextEditorControl textEditor;

	private IDocument document;

	private int immediateMarkLimit = 3;

	private bool markVisibleOnly = true;

	private int markVisibleAdditional = 5;

	private int directMark;

	public TextEditorControl TextEditor => textEditor;

	public IDocument Document => document;

	protected int ImmediateMarkLimit
	{
		get
		{
			return immediateMarkLimit;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", value, "value must be >= 0");
			}
			immediateMarkLimit = value;
		}
	}

	protected bool MarkVisibleOnly
	{
		get
		{
			return markVisibleOnly;
		}
		set
		{
			if (markVisibleOnly != value)
			{
				if (textEditor != null)
				{
					throw new InvalidOperationException("Cannot change value after initialization");
				}
				markVisibleOnly = value;
			}
		}
	}

	public int MarkVisibleAdditional
	{
		get
		{
			return markVisibleAdditional;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", value, "value must be >= 0");
			}
			markVisibleAdditional = value;
		}
	}

	protected abstract void MarkWords(int lineNumber, LineSegment currentLine, List<TextWord> words);

	public virtual void Initialize(TextEditorControl textEditor)
	{
		if (textEditor == null)
		{
			throw new ArgumentNullException("textEditor");
		}
		if (this.textEditor != null)
		{
			throw new InvalidOperationException("Already initialized");
		}
		this.textEditor = textEditor;
		document = textEditor.Document;
	}

	public virtual void Dispose()
	{
		textEditor = null;
		document = null;
	}

	public virtual void BeginUpdate(IDocument document, IList<LineSegment> inputLines)
	{
		if (this.document == null)
		{
			throw new InvalidOperationException("Not initialized");
		}
		if (document != this.document)
		{
			throw new InvalidOperationException("document != this.document");
		}
		if (inputLines == null)
		{
			lock (lockObject)
			{
				outstanding.Clear();
				return;
			}
		}
		directMark = ((inputLines.Count <= immediateMarkLimit) ? inputLines.Count : 0);
	}

	public virtual void EndUpdate()
	{
	}

	void IAdvancedHighlighter.MarkLine(int lineNumber, LineSegment currentLine, List<TextWord> words)
	{
		if (directMark > 0)
		{
			directMark--;
			MarkWords(lineNumber, currentLine, words);
			return;
		}
		lock (lockObject)
		{
			outstanding[currentLine] = words;
		}
	}

	protected virtual void MarkOutstanding()
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			throw new InvalidOperationException("Invoke required");
		}
		IEnumerable<KeyValuePair<LineSegment, List<TextWord>>> enumerable;
		lock (lockObject)
		{
			enumerable = outstanding;
			outstanding = new Dictionary<LineSegment, List<TextWord>>();
		}
		foreach (KeyValuePair<LineSegment, List<TextWord>> item in enumerable)
		{
			if (item.Key.IsDeleted)
			{
				continue;
			}
			int offset = item.Key.Offset;
			if (offset < 0 || offset >= document.TextLength)
			{
				continue;
			}
			int lineNumberForOffset = document.GetLineNumberForOffset(offset);
			if (markVisibleOnly && !IsVisible(lineNumberForOffset))
			{
				lock (lockObject)
				{
					outstanding[item.Key] = item.Value;
				}
			}
			else
			{
				MarkWords(lineNumberForOffset, item.Key, item.Value);
			}
		}
	}

	private bool IsVisible(int lineNumber)
	{
		TextView textView = textEditor.ActiveTextAreaControl.TextArea.TextView;
		int firstVisibleLine = textView.FirstVisibleLine;
		if (lineNumber < firstVisibleLine - markVisibleAdditional)
		{
			return false;
		}
		int firstLogicalLine = document.GetFirstLogicalLine(textView.FirstPhysicalLine + textView.VisibleLineCount);
		if (lineNumber > firstLogicalLine + markVisibleAdditional)
		{
			return false;
		}
		return document.GetVisibleLine(lineNumber) != document.GetVisibleLine(lineNumber - 1);
	}
}
