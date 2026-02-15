using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class ErrorDrawer : IDisposable
{
	private TextEditorControl textEditor;

	private bool isDisposed;

	private bool requireTextEditorRefresh;

	public ErrorDrawer(TextEditorControl textEditor)
	{
		this.textEditor = textEditor;
		TaskService.Added += OnAdded;
		TaskService.Removed += OnRemoved;
		TaskService.Cleared += OnCleared;
		TaskService.InUpdateChanged += OnInUpdateChanged;
		textEditor.FileNameChanged += SetErrors;
		DebuggerService.DebugStarted += OnDebugStarted;
		DebuggerService.DebugStopped += OnDebugStopped;
	}

	private void RefreshTextEditor()
	{
		if (TaskService.InUpdate)
		{
			requireTextEditorRefresh = true;
		}
		else
		{
			textEditor.Refresh();
		}
	}

	private void OnInUpdateChanged(object sender, EventArgs e)
	{
		if (requireTextEditorRefresh)
		{
			requireTextEditorRefresh = false;
			textEditor.Refresh();
		}
	}

	public void Dispose()
	{
		if (!isDisposed)
		{
			isDisposed = true;
			TaskService.Added -= OnAdded;
			TaskService.Removed -= OnRemoved;
			TaskService.Cleared -= OnCleared;
			TaskService.InUpdateChanged -= OnInUpdateChanged;
			textEditor.FileNameChanged -= SetErrors;
			DebuggerService.DebugStarted -= OnDebugStarted;
			DebuggerService.DebugStopped -= OnDebugStopped;
			ClearErrors();
		}
	}

	private void OnDebugStarted(object sender, EventArgs e)
	{
		ClearErrors();
	}

	private void OnDebugStopped(object sender, EventArgs e)
	{
		foreach (Task task in TaskService.Tasks)
		{
			AddTask(task, refresh: false);
		}
		textEditor.Refresh();
	}

	private void OnAdded(object sender, TaskEventArgs e)
	{
		AddTask(e.Task, refresh: true);
	}

	private void OnRemoved(object sender, TaskEventArgs e)
	{
		Task task = e.Task;
		foreach (TextMarker item in textEditor.Document.MarkerStrategy.TextMarker)
		{
			if (item is VisualError visualError && visualError.Task == task)
			{
				textEditor.Document.MarkerStrategy.RemoveMarker(item);
				RefreshTextEditor();
				break;
			}
		}
	}

	private void OnCleared(object sender, EventArgs e)
	{
		if (ClearErrors())
		{
			RefreshTextEditor();
		}
	}

	private bool ClearErrors()
	{
		bool removed = false;
		textEditor.Document.MarkerStrategy.RemoveAll(delegate(TextMarker marker)
		{
			if (marker is VisualError)
			{
				removed = true;
				return true;
			}
			return false;
		});
		return removed;
	}

	private bool CheckTask(Task task)
	{
		if (textEditor.FileName == null)
		{
			return false;
		}
		if (task.FileName == null || task.FileName.Length == 0 || task.Column < 0)
		{
			return false;
		}
		if (task.TaskType != TaskType.Warning && task.TaskType != TaskType.Error)
		{
			return false;
		}
		return FileUtility.IsEqualFileName(task.FileName, textEditor.FileName);
	}

	private void AddTask(Task task, bool refresh)
	{
		if (!CheckTask(task) || task.Line < 0 || task.Line >= textEditor.Document.TotalNumberOfLines)
		{
			return;
		}
		LineSegment lineSegment = textEditor.Document.GetLineSegment(task.Line);
		int offset = lineSegment.Offset + task.Column;
		int num = 1;
		if (lineSegment.Words != null)
		{
			foreach (TextWord word in lineSegment.Words)
			{
				if (task.Column == word.Offset)
				{
					num = word.Length;
					break;
				}
			}
		}
		if (num == 1 && task.Column < lineSegment.Length)
		{
			num = 2;
		}
		textEditor.Document.MarkerStrategy.AddMarker(new VisualError(offset, num, task));
		if (refresh)
		{
			RefreshTextEditor();
		}
	}

	private void SetErrors(object sender, EventArgs e)
	{
		ClearErrors();
		foreach (Task task in TaskService.Tasks)
		{
			AddTask(task, refresh: false);
		}
		textEditor.Refresh();
	}
}
