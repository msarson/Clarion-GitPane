using System;
using System.IO;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public class Task : ICloneable
{
	public const string DefaultContextMenuAddInTreeEntry = "/SharpDevelop/Pads/ErrorList/TaskContextMenu";

	private string description;

	private string fileName;

	private TaskType type;

	private int line;

	private int column;

	private string contextMenuAddInTreeEntry = "/SharpDevelop/Pads/ErrorList/TaskContextMenu";

	private object tag;

	public static EventHandler<NewTaskEventArgs> NewTaskEvent;

	public static EventHandler<NewCommentTagTaskEventArgs> NewCommentTagTaskEvent;

	public int Line
	{
		get
		{
			return line;
		}
		set
		{
			line = value;
		}
	}

	public int Column
	{
		get
		{
			return column;
		}
		set
		{
			column = value;
		}
	}

	public string Description => description;

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}

	public TaskType TaskType => type;

	public string ContextMenuAddInTreeEntry
	{
		get
		{
			return contextMenuAddInTreeEntry;
		}
		set
		{
			contextMenuAddInTreeEntry = value;
		}
	}

	public object Tag
	{
		get
		{
			return tag;
		}
		set
		{
			tag = value;
		}
	}

	public override string ToString()
	{
		return $"[Task:File={fileName}, Line={line}, Column={column}, Type={type}, Description={description}";
	}

	public Task(string fileName, string description, int column, int line, TaskType type)
	{
		this.type = type;
		this.fileName = fileName;
		this.description = description.Trim();
		this.column = column;
		this.line = line;
	}

	public static Task NewTask(BuildError error)
	{
		NewTaskEventArgs e = new NewTaskEventArgs(error);
		if (NewTaskEvent != null)
		{
			NewTaskEvent(null, e);
		}
		if (e.Task == null)
		{
			return new Task(error);
		}
		return e.Task;
	}

	public static Task NewCommentTagTask(string fileName, TagComment tag)
	{
		NewCommentTagTaskEventArgs e = new NewCommentTagTaskEventArgs(fileName, tag);
		if (NewCommentTagTaskEvent != null)
		{
			NewCommentTagTaskEvent(null, e);
		}
		if (e.Task == null)
		{
			return new Task(fileName, tag.Key + tag.CommentString, tag.Region.BeginColumn - 1, tag.Region.BeginLine - 1, TaskType.Comment);
		}
		return e.Task;
	}

	public Task(BuildError error)
	{
		type = (error.IsWarning ? TaskType.Warning : TaskType.Error);
		column = Math.Max(error.Column - 1, 0);
		line = Math.Max(error.Line - 1, 0);
		fileName = error.FileName;
		if (string.IsNullOrEmpty(error.ErrorCode))
		{
			description = error.ErrorText;
		}
		else
		{
			description = error.ErrorText + " (" + error.ErrorCode + ")";
		}
		if (error.ContextMenuAddInTreeEntry != null)
		{
			contextMenuAddInTreeEntry = error.ContextMenuAddInTreeEntry;
		}
		tag = error.Tag;
	}

	public virtual void JumpToPosition()
	{
		if (File.Exists(fileName))
		{
			FileService.JumpToFilePosition(fileName, line, column);
		}
	}

	public object Clone()
	{
		return new Task(FileName, Description, Column, Line, TaskType);
	}
}
