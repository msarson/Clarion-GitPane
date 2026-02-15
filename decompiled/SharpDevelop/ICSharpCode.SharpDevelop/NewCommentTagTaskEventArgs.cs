using System;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop;

public class NewCommentTagTaskEventArgs : EventArgs
{
	private Task task;

	private TagComment tag;

	private string fileName;

	public Task Task
	{
		get
		{
			return task;
		}
		set
		{
			task = value;
		}
	}

	public TagComment TagComment => tag;

	public string FileName => fileName;

	public NewCommentTagTaskEventArgs(string fileName, TagComment tag)
	{
		this.tag = tag;
		this.fileName = fileName;
		task = null;
	}
}
