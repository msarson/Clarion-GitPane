using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class SdStatusBar : StatusStrip, IProgressNotificationCenter
{
	private ToolStripProgressBar statusProgressBar = new ToolStripProgressBar();

	private ToolStripStatusLabel jobNamePanel = new ToolStripStatusLabel();

	private ToolStripStatusLabel txtStatusBarPanel = new ToolStripStatusLabel();

	private ToolStripStatusLabel modeStatusBarPanel = new ToolStripStatusLabel();

	private ToolStripStatusLabel springLabel = new ToolStripStatusLabel();

	private ToolStripStatusLabel cursorStatusBarPanelLine = new ToolStripStatusLabel();

	private ToolStripStatusLabel cursorStatusBarPanelColumn = new ToolStripStatusLabel();

	private ToolStripStatusLabel cursorStatusBarPanelChar = new ToolStripStatusLabel();

	private bool _ShowMessage = true;

	private bool _ShowProgress = true;

	private ProgressTaskControlManager _ProgressTaskControlManager;

	private IProgressNotificationCenter _IProgressTaskControlManager;

	private string currentMessage;

	private static readonly object Locker = new object();

	private static string LockerName;

	public ToolStripStatusLabel CursorStatusBarPanelLine => cursorStatusBarPanelLine;

	public ToolStripStatusLabel CursorStatusBarPanelColumn => cursorStatusBarPanelColumn;

	public ToolStripStatusLabel CursorStatusBarPanelChar => cursorStatusBarPanelChar;

	public ToolStripStatusLabel ModeStatusBarPanel => modeStatusBarPanel;

	private bool _ShowNotifications => _IProgressTaskControlManager.ShowNotifications;

	bool IProgressNotificationCenter.ShowNotifications => _ShowNotifications;

	public SdStatusBar()
	{
		_ShowMessage = PropertyService.Get("ShowStatusMessage", _ShowMessage);
		_ShowProgress = PropertyService.Get("ShowStatusProgress", _ShowProgress);
		Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
		springLabel.Spring = true;
		cursorStatusBarPanelLine.AutoSize = false;
		cursorStatusBarPanelLine.Width = 90;
		cursorStatusBarPanelLine.TextAlign = ContentAlignment.MiddleLeft;
		cursorStatusBarPanelColumn.AutoSize = false;
		cursorStatusBarPanelColumn.Width = 78;
		cursorStatusBarPanelColumn.TextAlign = ContentAlignment.MiddleLeft;
		cursorStatusBarPanelChar.AutoSize = false;
		cursorStatusBarPanelChar.Width = 75;
		cursorStatusBarPanelChar.TextAlign = ContentAlignment.MiddleLeft;
		modeStatusBarPanel.AutoSize = false;
		modeStatusBarPanel.Width = 35;
		modeStatusBarPanel.TextAlign = ContentAlignment.MiddleLeft;
		statusProgressBar.Visible = false;
		statusProgressBar.Width = 100;
		txtStatusBarPanel.TextAlign = ContentAlignment.MiddleLeft;
		jobNamePanel.TextAlign = ContentAlignment.MiddleLeft;
		Items.AddRange(new ToolStripItem[8] { txtStatusBarPanel, springLabel, jobNamePanel, statusProgressBar, cursorStatusBarPanelLine, cursorStatusBarPanelColumn, cursorStatusBarPanelChar, modeStatusBarPanel });
		_ProgressTaskControlManager = new ProgressTaskControlManager(this);
		_IProgressTaskControlManager = _ProgressTaskControlManager;
	}

	public void Init()
	{
		_ProgressTaskControlManager.Init();
		_ProgressTaskControlManager.Show();
	}

	protected override void OnParentChanged(EventArgs e)
	{
		base.OnParentChanged(e);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		if (_ShowMessage)
		{
			WorkbenchSingleton.SafeThreadCall(UpdateText);
		}
	}

	public void ShowErrorMessage(string message)
	{
		if (_ShowMessage)
		{
			SetMessage("Error : " + message);
		}
	}

	public void ShowErrorMessage(Image image, string message)
	{
		if (_ShowMessage)
		{
			SetMessage(image, "Error : " + message);
		}
	}

	public void SetMessage(string message)
	{
		if (_ShowMessage)
		{
			SetMessage(message, highlighted: false, force: false);
		}
	}

	private void DoSetMessage(bool highlighted)
	{
		if (highlighted)
		{
			txtStatusBarPanel.BackColor = SystemColors.Highlight;
			txtStatusBarPanel.ForeColor = Color.White;
		}
		else if (txtStatusBarPanel.BackColor == SystemColors.Highlight)
		{
			txtStatusBarPanel.BackColor = SystemColors.Control;
			txtStatusBarPanel.ForeColor = SystemColors.ControlText;
		}
		UpdateText();
	}

	internal void SetMessage(string message, bool highlighted, bool force)
	{
		if (!_ShowMessage || !(currentMessage != message))
		{
			return;
		}
		currentMessage = message;
		if (base.IsHandleCreated)
		{
			if (force)
			{
				WorkbenchSingleton.SafeThreadCall(DoSetMessage, highlighted);
			}
			else
			{
				WorkbenchSingleton.SafeThreadAsyncCall(DoSetMessage, highlighted);
			}
		}
	}

	public void SetMessage(string message, bool highlighted)
	{
		if (_ShowMessage)
		{
			SetMessage(message, highlighted, force: false);
		}
	}

	private void UpdateText()
	{
		if (_ShowMessage && !base.IsDisposed)
		{
			txtStatusBarPanel.Text = currentMessage;
			Refresh();
		}
	}

	public void SetMessage(Image image, string message)
	{
		if (_ShowMessage)
		{
			SetMessage(message);
		}
	}

	public void ClearCaretancursorText()
	{
		if (_ShowMessage)
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				WorkbenchSingleton.SafeThreadAsyncCall(ClearCaretancursorText);
				return;
			}
			cursorStatusBarPanelLine.Text = "";
			cursorStatusBarPanelColumn.Text = "";
			cursorStatusBarPanelChar.Text = "";
			ModeStatusBarPanel.Text = "";
			Refresh();
		}
	}

	public void SetCaretPositionText(string lineMessage, string colMessage, string charOffsetMessage)
	{
		if (_ShowMessage)
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				WorkbenchSingleton.SafeThreadAsyncCall(SetCaretPositionText, lineMessage, colMessage, charOffsetMessage);
				return;
			}
			cursorStatusBarPanelLine.Text = lineMessage;
			cursorStatusBarPanelColumn.Text = colMessage;
			cursorStatusBarPanelChar.Text = charOffsetMessage;
			Refresh();
		}
	}

	public void SetInsertModeText(string message)
	{
		if (_ShowMessage)
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				WorkbenchSingleton.SafeThreadAsyncCall(SetInsertModeText, message);
				return;
			}
			ModeStatusBarPanel.Text = message;
			Refresh();
		}
	}

	public void SetVisible(bool value)
	{
		if (_ShowProgress)
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				WorkbenchSingleton.SafeThreadAsyncCall(SetVisible, value);
				return;
			}
			base.Visible = value;
			Refresh();
		}
	}

	public bool GetVisible()
	{
		if (_ShowProgress)
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				return WorkbenchSingleton.SafeThreadFunction(GetVisible);
			}
			return base.Visible;
		}
		return false;
	}

	private void _BeginTask(string taskName, int totalWork, bool allowCancel)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_BeginTask, taskName, totalWork, allowCancel);
			return;
		}
		WorkbenchSingleton.DoEvents();
		_IProgressTaskControlManager.BeginTask(taskName, totalWork, allowCancel);
	}

	private void _BeginTask(string taskName, string initialTaskText, int totalWork, bool allowCancel)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_BeginTask, taskName, initialTaskText, totalWork, allowCancel);
			return;
		}
		WorkbenchSingleton.DoEvents();
		_IProgressTaskControlManager.BeginTask(taskName, initialTaskText, totalWork, allowCancel);
	}

	private void _ShowNotification(string taskName, string taskText)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_ShowNotification, taskName, taskText);
			return;
		}
		WorkbenchSingleton.DoEvents();
		_IProgressTaskControlManager.ShowNotification(taskName, taskText);
	}

	private void _SetTaskTextAndWork(string taskName, string taskText, int workValue)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_SetTaskTextAndWork, taskName, taskText, workValue);
		}
		else
		{
			_IProgressTaskControlManager.SetTaskTextAndWork(taskName, taskText, workValue);
		}
	}

	private int _GetWorkDone(string taskName)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction(_GetWorkDone, taskName);
		}
		return _IProgressTaskControlManager.GetWorkDone(taskName);
	}

	private void _SetWorkDone(string taskName, int value)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_SetWorkDone, taskName, value);
		}
		else
		{
			_IProgressTaskControlManager.SetWorkDone(taskName, value);
		}
	}

	private void _SetTaskText(string taskName, string taskText)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_SetTaskText, taskName, taskText);
		}
		else
		{
			_IProgressTaskControlManager.SetTaskText(taskName, taskText);
		}
	}

	private void _IncreseWorkDoneBy(string taskName, int value)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_IncreseWorkDoneBy, taskName, value);
		}
		else
		{
			_IProgressTaskControlManager.IncreaseWorkDoneBy(taskName, value);
		}
	}

	private void _DecreseWorkDoneBy(string taskName, int value)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_DecreseWorkDoneBy, taskName, value);
		}
		else
		{
			_IProgressTaskControlManager.DecreaseWorkDoneBy(taskName, value);
		}
	}

	private bool _IsWorkDone(string taskName)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction(_IsWorkDone, taskName);
		}
		return _IProgressTaskControlManager.IsWorkDone(taskName);
	}

	private void _Done(string taskName)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_Done, taskName);
		}
		else
		{
			_IProgressTaskControlManager.Done(taskName);
		}
	}

	private bool _TaskStarted(string taskName)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction(_TaskStarted, taskName);
		}
		return _IProgressTaskControlManager.TaskStarted(taskName);
	}

	private bool _GetIsCancelled(string taskName)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction(_GetIsCancelled, taskName);
		}
		return _IProgressTaskControlManager.GetIsCancelled(taskName);
	}

	private void _SetIsCancelled(string taskName, bool value)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall(_SetIsCancelled, taskName, value);
		}
		else
		{
			_IProgressTaskControlManager.SetIsCancelled(taskName, value);
		}
	}

	void IProgressNotificationCenter.BeginTask(string taskName, int totalWork, bool allowCancel)
	{
		_BeginTask(taskName, totalWork, allowCancel);
	}

	void IProgressNotificationCenter.BeginTask(string taskName, string initialTaskText, int totalWork, bool allowCancel)
	{
		_BeginTask(taskName, initialTaskText, totalWork, allowCancel);
	}

	void IProgressNotificationCenter.ShowNotification(string taskName, string taskText)
	{
		_ShowNotification(taskName, taskText);
	}

	void IProgressNotificationCenter.SetTaskTextAndWork(string taskName, string taskText, int workValue)
	{
		_SetTaskTextAndWork(taskName, taskText, workValue);
	}

	int IProgressNotificationCenter.GetWorkDone(string taskName)
	{
		return _GetWorkDone(taskName);
	}

	void IProgressNotificationCenter.SetWorkDone(string taskName, int value)
	{
		_SetWorkDone(taskName, value);
	}

	void IProgressNotificationCenter.SetTaskText(string taskName, string taskText)
	{
		_SetTaskText(taskName, taskText);
	}

	void IProgressNotificationCenter.IncreaseWorkDoneBy(string taskName, int value)
	{
		_IncreseWorkDoneBy(taskName, value);
	}

	void IProgressNotificationCenter.DecreaseWorkDoneBy(string taskName, int value)
	{
		_DecreseWorkDoneBy(taskName, value);
	}

	bool IProgressNotificationCenter.IsWorkDone(string taskName)
	{
		return _IsWorkDone(taskName);
	}

	bool IProgressNotificationCenter.TaskStarted(string taskName)
	{
		return _TaskStarted(taskName);
	}

	void IProgressNotificationCenter.Done(string taskName)
	{
		_Done(taskName);
	}

	bool IProgressNotificationCenter.GetIsCancelled(string taskName)
	{
		return _GetIsCancelled(taskName);
	}

	void IProgressNotificationCenter.SetIsCancelled(string taskName, bool value)
	{
		_SetIsCancelled(taskName, value);
	}
}
