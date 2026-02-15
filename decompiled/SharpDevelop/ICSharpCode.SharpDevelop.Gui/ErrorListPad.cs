using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class ErrorListPad : AbstractPadContent, IClipboardHandler
{
	private static ErrorListPad instance;

	private ToolStrip toolStrip;

	private Panel contentPanel;

	private TaskView taskView;

	private bool showWarnings = true;

	private bool showErrors = true;

	private bool showMessages = true;

	public BuildResults BuildResults;

	public static ErrorListPad Instance => instance;

	public bool ShowErrors
	{
		get
		{
			return showErrors;
		}
		set
		{
			showErrors = value;
			InternalShowResults();
		}
	}

	public bool ShowMessages
	{
		get
		{
			return showMessages;
		}
		set
		{
			showMessages = value;
			InternalShowResults();
		}
	}

	public bool ShowWarnings
	{
		get
		{
			return showWarnings;
		}
		set
		{
			showWarnings = value;
			InternalShowResults();
		}
	}

	public static bool ShouldShowAfterBuild
	{
		get
		{
			return PropertyService.Get("SharpDevelop.ShowErrorListAfterBuild", defaultValue: true);
		}
		set
		{
			PropertyService.Set("SharpDevelop.ShowErrorListAfterBuild", value);
		}
	}

	public static bool ShowAndPinIfErrors
	{
		get
		{
			return PropertyService.Get("SharpDevelop.ShowErrorListAndPinIfErrors", defaultValue: true);
		}
		set
		{
			PropertyService.Set("SharpDevelop.ShowErrorListAndPinIfErrors", value);
		}
	}

	public override Control Control => contentPanel;

	public List<Task> Tasks => taskView.Tasks;

	public bool EnableCut => false;

	public bool EnableCopy => taskView.TaskIsSelected;

	public bool EnablePaste => false;

	public bool EnableDelete => taskView.Tasks.Count > 0;

	public bool EnableSelectAll => taskView.Tasks.Count > 0;

	internal bool NextValid => taskView.NextValid;

	internal bool PreviousValid => taskView.PreviousValid;

	public ErrorListPad()
	{
		instance = this;
		taskView = new TaskView("ErrorList");
		contentPanel = new Panel();
		Control.ParentChanged += OnParentChanged;
		taskView.ItemSelectionChanged += TaskSelected;
		taskView.ItemActivate += TaskActivated;
		taskView.ColumnClick += TaskColumnClicked;
		RedrawContent();
		TaskService.Cleared += TaskServiceCleared;
		TaskService.Added += TaskServiceAdded;
		TaskService.Removed += TaskServiceRemoved;
		EventHandler value = delegate
		{
			if (!TaskService.InUpdate)
			{
				InternalShowResults();
			}
		};
		TaskService.InUpdateChanged += value;
		ProjectService.EndBuild += ProjectServiceEndBuild;
		ProjectService.SolutionLoaded += OnCombineOpen;
		ProjectService.SolutionClosed += OnCombineClosed;
		contentPanel.Controls.Add(taskView);
		if (WorkbenchSingleton.MainForm != null)
		{
			WorkbenchSingleton.MainForm.FormClosing += OnMainFormFormClosing;
		}
		toolStrip = ToolbarService.CreateToolStrip(this, "/SharpDevelop/Pads/ErrorList/Toolbar");
		toolStrip.Stretch = true;
		toolStrip.GripStyle = ToolStripGripStyle.Hidden;
		contentPanel.Controls.Add(toolStrip);
		InternalShowResults();
		UpdateToolstripStatus();
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

	private void TaskColumnClicked(object sender, ColumnClickEventArgs e)
	{
		UpdateToolstripStatus();
	}

	private void TaskActivated(object sender, EventArgs e)
	{
		UpdateToolstripStatus();
		if (taskView.FocusedItem != null)
		{
			if (taskView.SelectedTask.TaskType == TaskType.Error)
			{
				StatusBarService.SetMessage("Error: " + taskView.SelectedTask.Description, highlighted: true);
			}
			else
			{
				StatusBarService.SetMessage("Warning: " + taskView.SelectedTask.Description);
			}
		}
	}

	private void TaskSelected(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		UpdateToolstripStatus();
	}

	public override void RedrawContent()
	{
		taskView.RefreshColumnNames();
	}

	public void UpdateResults(IEnumerable<Task> taskSet)
	{
		taskView.UpdateResults(taskSet);
	}

	private void OnCombineOpen(object sender, SolutionEventArgs e)
	{
		taskView.ClearTasks();
		StatusBarService.ClearMessage();
		UpdateToolstripStatus();
	}

	private void OnCombineClosed(object sender, EventArgs e)
	{
		try
		{
			taskView.StoreColumnWidths();
			taskView.ClearTasks();
			UpdateToolstripStatus();
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}

	private void ProjectServiceEndBuild(object sender, EventArgs e)
	{
		ShowAfterBuild();
	}

	public static void ShowAfterBuild()
	{
		if (TaskService.SomethingWentWrong && ShouldShowAfterBuild)
		{
			WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(typeof(ErrorListPad).FullName);
			Instance.UpdateToolstripStatus();
			ShowIfNotEmpty(ShowAndPinIfErrors);
		}
	}

	public static void ShowIfNotEmpty()
	{
		ShowIfNotEmpty(forcePin: false);
	}

	public static void ShowIfNotEmpty(bool forcePin)
	{
		if (TaskService.SomethingWentWrong)
		{
			if (forcePin || WorkbenchSingleton.Workbench.WorkbenchLayout.ActiveWorkbenchwindow == null || WorkbenchSingleton.Workbench.WorkbenchLayout.ActiveContent == null)
			{
				WorkbenchSingleton.Workbench.WorkbenchLayout.ActivateAndDockPad(typeof(ErrorListPad).FullName);
				Instance.ShowErrors = true;
				Instance.taskView.AutoResizeColumnWidths();
				Instance.taskView.Refresh();
			}
			else
			{
				WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(typeof(ErrorListPad).FullName);
			}
			ShowErrorList();
		}
	}

	public static void ShowIfErrors()
	{
		if (TaskService.HasCriticalErrors(treatWarningsAsErrors: false))
		{
			if (ShowAndPinIfErrors || WorkbenchSingleton.Workbench.WorkbenchLayout.ActiveWorkbenchwindow == null || WorkbenchSingleton.Workbench.WorkbenchLayout.ActiveContent == null)
			{
				WorkbenchSingleton.Workbench.WorkbenchLayout.ActivateAndDockPad(typeof(ErrorListPad).FullName);
				Instance.ShowErrors = true;
				Instance.taskView.AutoResizeColumnWidths();
				Instance.taskView.Refresh();
			}
			else
			{
				WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(typeof(ErrorListPad).FullName);
			}
			ShowErrorList();
		}
	}

	public static void ShowErrorList()
	{
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(ErrorListPad));
		if (pad != null)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(pad.BringPadToFront);
		}
	}

	public string GetLastErrorDescription()
	{
		return TaskService.GetLastErrorDescription();
	}

	private void AddTask(Task task)
	{
		switch (task.TaskType)
		{
		default:
			return;
		case TaskType.Warning:
			if (!ShowWarnings)
			{
				return;
			}
			break;
		case TaskType.Error:
			if (!ShowErrors)
			{
				return;
			}
			break;
		case TaskType.Message:
			if (!ShowMessages)
			{
				return;
			}
			break;
		}
		taskView.AddTask(task);
	}

	private void TaskServiceCleared(object sender, EventArgs e)
	{
		if (!TaskService.InUpdate)
		{
			taskView.ClearTasks();
			StatusBarService.ClearMessage();
			UpdateToolstripStatus();
		}
	}

	private void TaskServiceAdded(object sender, TaskEventArgs e)
	{
		if (!TaskService.InUpdate)
		{
			AddTask(e.Task);
			UpdateToolstripStatus();
		}
	}

	private void TaskServiceRemoved(object sender, TaskEventArgs e)
	{
		if (!TaskService.InUpdate)
		{
			taskView.RemoveTask(e.Task);
			UpdateToolstripStatus();
		}
	}

	private void UpdateToolstripStatus()
	{
		ToolbarService.UpdateToolbar(toolStrip);
		ToolbarService.UpdateToolbarText(toolStrip);
	}

	private void InternalShowResults()
	{
		taskView.BeginUpdate();
		taskView.ClearTasks();
		StatusBarService.ClearMessage();
		foreach (Task task in TaskService.Tasks)
		{
			AddTask(task);
		}
		taskView.EndUpdate();
		UpdateToolstripStatus();
	}

	public void Cut()
	{
	}

	public void Paste()
	{
	}

	public void Delete()
	{
		TaskService.Clear();
		StatusBarService.ClearMessage();
	}

	public void Copy()
	{
		taskView.CopySelectionToClipboard();
	}

	public void SelectAll()
	{
		taskView.SelectAll();
	}

	public void SelectNext()
	{
		taskView.SelectNext();
	}

	public void SelectPrevious()
	{
		taskView.SelectPrevious();
	}
}
