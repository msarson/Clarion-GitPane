using System;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class TaskListPad : AbstractPadContent, IClipboardHandler
{
	private TaskView taskView = new TaskView("TaskList");

	public override Control Control => taskView;

	public bool EnableCut => false;

	public bool EnableCopy => taskView.TaskIsSelected;

	public bool EnablePaste => false;

	public bool EnableDelete => false;

	public bool EnableSelectAll => true;

	public TaskListPad()
	{
		Control.ParentChanged += OnParentChanged;
		RedrawContent();
		TaskService.Cleared += TaskServiceCleared;
		TaskService.Added += TaskServiceAdded;
		TaskService.Removed += TaskServiceRemoved;
		ProjectService.SolutionLoaded += OnCombineOpen;
		ProjectService.SolutionClosed += OnCombineClosed;
		if (WorkbenchSingleton.MainForm != null)
		{
			WorkbenchSingleton.MainForm.FormClosing += OnMainFormFormClosing;
		}
		InternalShowResults(null, null);
	}

	private void OnMainFormFormClosing(object sender, FormClosingEventArgs e)
	{
		if (taskView != null)
		{
			try
			{
				taskView.StoreColumnWidths(force: true);
			}
			catch
			{
			}
		}
		WorkbenchSingleton.MainForm.FormClosing -= OnMainFormFormClosing;
	}

	private void OnParentChanged(object sender, EventArgs e)
	{
		if (Control.Parent != null)
		{
			taskView.RestoreColumnWidths();
			Control.Parent.Resize += OnParentResize;
		}
	}

	private void OnParentResize(object sender, EventArgs e)
	{
		taskView.AutoResizeColumnWidths();
	}

	public override void RedrawContent()
	{
		taskView.RefreshColumnNames();
	}

	private void OnCombineOpen(object sender, SolutionEventArgs e)
	{
		taskView.ClearTasks();
	}

	private void OnCombineClosed(object sender, EventArgs e)
	{
		taskView.ClearTasks();
	}

	private void TaskServiceCleared(object sender, EventArgs e)
	{
		taskView.ClearTasks();
	}

	private void TaskServiceAdded(object sender, TaskEventArgs e)
	{
		if (e.Task.TaskType == TaskType.Comment)
		{
			taskView.AddTask(e.Task);
		}
	}

	private void TaskServiceRemoved(object sender, TaskEventArgs e)
	{
		if (e.Task.TaskType == TaskType.Comment)
		{
			taskView.RemoveTask(e.Task);
		}
	}

	private void InternalShowResults(object sender, EventArgs e)
	{
		taskView.UpdateResults(TaskService.CommentTasks);
	}

	public void ShowResults(object sender, EventArgs e)
	{
		taskView.Invoke(new EventHandler(InternalShowResults));
	}

	public void Cut()
	{
	}

	public void Paste()
	{
	}

	public void Delete()
	{
	}

	public void Copy()
	{
		taskView.CopySelectionToClipboard();
	}

	public void SelectAll()
	{
		taskView.SelectAll();
	}
}
