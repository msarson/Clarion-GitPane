using System;
using System.ComponentModel;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

[TypeConverter(typeof(BookmarkConverter))]
public class SDBookmark : Bookmark
{
	private string fileName;

	private bool isSaved = true;

	private bool isVisibleInBookmarkPad = true;

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			if (fileName != value)
			{
				fileName = value;
				OnFileNameChanged(EventArgs.Empty);
			}
		}
	}

	public bool IsSaved
	{
		get
		{
			return isSaved;
		}
		set
		{
			isSaved = value;
		}
	}

	public bool IsVisibleInBookmarkPad
	{
		get
		{
			return isVisibleInBookmarkPad;
		}
		set
		{
			isVisibleInBookmarkPad = value;
		}
	}

	public event EventHandler FileNameChanged;

	public event EventHandler LineNumberChanged;

	public SDBookmark(string fileName, IDocument document, int lineNumber)
		: base(document, lineNumber)
	{
		this.fileName = fileName;
	}

	protected virtual void OnFileNameChanged(EventArgs e)
	{
		if (this.FileNameChanged != null)
		{
			this.FileNameChanged(this, e);
		}
	}

	internal void RaiseLineNumberChanged()
	{
		if (this.LineNumberChanged != null)
		{
			this.LineNumberChanged(this, EventArgs.Empty);
		}
	}
}
