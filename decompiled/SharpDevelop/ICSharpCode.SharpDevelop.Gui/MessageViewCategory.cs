using System;
using System.IO;
using System.Text;

namespace ICSharpCode.SharpDevelop.Gui;

public class MessageViewCategory
{
	private string category;

	private string displayCategory;

	private StringBuilder textBuilder = new StringBuilder();

	private bool suspended;

	public string Category => category;

	public string DisplayCategory => displayCategory;

	public string Text
	{
		get
		{
			lock (textBuilder)
			{
				return textBuilder.ToString();
			}
		}
	}

	public event TextEventHandler TextAppended;

	public event TextEventHandler TextSet;

	public event EventHandler Cleared;

	public MessageViewCategory(string category)
		: this(category, category)
	{
	}

	public MessageViewCategory(string category, string displayCategory)
	{
		this.category = category;
		this.displayCategory = displayCategory;
		suspended = false;
	}

	public void AppendLine(string text)
	{
		AppendText(text + Environment.NewLine);
	}

	public void AppendText(string text)
	{
		lock (textBuilder)
		{
			if (!suspended)
			{
				try
				{
					textBuilder.Append(text);
				}
				catch (OutOfMemoryException)
				{
					suspended = true;
				}
			}
		}
		OnTextAppended(new TextEventArgs(text));
	}

	public void SetText(string text)
	{
		lock (textBuilder)
		{
			suspended = false;
			textBuilder.Length = 0;
			textBuilder.Append(text);
		}
		OnTextSet(new TextEventArgs(text));
	}

	public void ClearText()
	{
		lock (textBuilder)
		{
			suspended = false;
			textBuilder.Length = 0;
		}
		OnCleared(EventArgs.Empty);
	}

	protected virtual void OnTextAppended(TextEventArgs e)
	{
		if (this.TextAppended != null)
		{
			this.TextAppended(this, e);
		}
	}

	protected virtual void OnCleared(EventArgs e)
	{
		if (this.Cleared != null)
		{
			this.Cleared(this, e);
		}
	}

	protected virtual void OnTextSet(TextEventArgs e)
	{
		if (this.TextSet != null)
		{
			this.TextSet(this, e);
		}
	}

	public virtual void JumpToPosition(string textLine)
	{
		FileLineReference fileLineReference = OutputTextLineParser.GetFileLineReference(textLine);
		if (fileLineReference != null)
		{
			FileService.JumpToFilePosition(Path.GetFullPath(fileLineReference.FileName), fileLineReference.Line, fileLineReference.Column);
		}
	}
}
