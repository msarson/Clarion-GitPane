using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ProgressTaskControlManager : UserControl, IProgressNotificationCenter
{
	private bool _ShowNotifications = true;

	private Control bottomControl;

	private StatusStrip statusStripControl;

	private Dictionary<string, IProgressNotificationTask> tasks = new Dictionary<string, IProgressNotificationTask>();

	private int savedHeight;

	private IContainer components;

	private FlowLayoutPanel flowLayoutPanel1;

	public bool ShowNotifications => _ShowNotifications;

	bool IProgressNotificationCenter.ShowNotifications => ShowNotifications;

	public ProgressTaskControlManager()
	{
		InitializeComponent();
		_ShowNotifications = PropertyService.Get("ShowNotification", _ShowNotifications);
		if (ShowNotifications)
		{
			base.ParentChanged += ProgressTaskControlManager_ParentChanged;
		}
	}

	public ProgressTaskControlManager(StatusStrip statusStripControl)
		: this()
	{
		SetParentForm(statusStripControl);
	}

	public void Init()
	{
		SetBottomControl();
	}

	private void SetParentForm(StatusStrip statusStripControl)
	{
		if (ShowNotifications)
		{
			this.statusStripControl = statusStripControl;
			SetBottomControl();
		}
	}

	private void SetBottomControl()
	{
		if (ShowNotifications && bottomControl == null && statusStripControl != null)
		{
			Form form = statusStripControl.FindForm();
			if (form != null)
			{
				form.Controls.Add(this);
				bottomControl = statusStripControl;
			}
		}
	}

	public void SetPosition()
	{
		if (!ShowNotifications)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		if (bottomControl != null)
		{
			if (bottomControl.Dock == DockStyle.Fill && bottomControl.Location.X == 0 && bottomControl.Location.Y == 0)
			{
				if (bottomControl.Parent != null)
				{
					num = bottomControl.Parent.Location.X + bottomControl.Width - base.Width - 2;
					num2 = bottomControl.Parent.Location.Y - base.Height - 2;
				}
			}
			else
			{
				num = bottomControl.Location.X + bottomControl.Width - base.Width - 2;
				num2 = bottomControl.Location.Y - base.Height - 2;
			}
			base.Location = new Point(num, num2);
			if (base.ParentForm.Controls.IndexOf(this) != 0)
			{
				base.ParentForm.Controls.SetChildIndex(this, 0);
			}
			ThisInvalidate();
		}
		else if (base.ParentForm != null)
		{
			num = base.ParentForm.Width - base.Width - 2;
			num2 = base.ParentForm.Height - base.Height - 2;
			base.Location = new Point(num, num2);
			ThisInvalidate();
		}
	}

	private void ProgressTaskControlManager_ParentChanged(object sender, EventArgs e)
	{
		if (base.ParentForm != null)
		{
			base.ParentForm.SizeChanged += ParentForm_SizeChanged;
			base.ParentForm.Move += ParentForm_Move;
		}
	}

	private void ThisRefresh()
	{
		if (ShowNotifications)
		{
			Refresh();
		}
	}

	private void ThisInvalidate()
	{
		if (ShowNotifications)
		{
			Invalidate();
		}
	}

	private void ParentForm_Move(object sender, EventArgs e)
	{
		SetPosition();
	}

	private void ParentForm_SizeChanged(object sender, EventArgs e)
	{
		SetPosition();
	}

	private void progressTaskControl1_Load(object sender, EventArgs e)
	{
	}

	void IProgressNotificationCenter.BeginTask(string taskName, int totalWork, bool allowCancel)
	{
		DoBeginTask(taskName, null, totalWork, allowCancel);
	}

	void IProgressNotificationCenter.BeginTask(string taskName, string taskText, int totalWork, bool allowCancel)
	{
		DoBeginTask(taskName, taskText, totalWork, allowCancel);
	}

	void IProgressNotificationCenter.ShowNotification(string taskName, string taskText)
	{
		DoBeginTask(taskName, taskText, 0, allowCancel: false);
	}

	private void DoBeginTask(string taskName, string taskText, int totalWork, bool allowCancel)
	{
		if (ShowNotifications)
		{
			SuspendLayout();
		}
		bool flag = WasTaskStarted(taskName);
		TaskDone(taskName);
		AddNewTask(taskName, taskText, totalWork, allowCancel);
		if (flag)
		{
			if (ShowNotifications)
			{
				base.Visible = true;
				Show();
			}
			ShowTask(taskName);
		}
		if (ShowNotifications)
		{
			ResumeLayout(performLayout: true);
			base.ParentForm.Update();
		}
	}

	private void ShowTask(string taskName)
	{
		if (WasTaskStarted(taskName))
		{
			tasks[taskName].Show();
			if (ShowNotifications)
			{
				ThisInvalidate();
			}
		}
	}

	private void AddNewTask(string taskName, string taskText, int totalWork, bool allowCancel)
	{
		if (ShowNotifications)
		{
			ProgressTaskControl progressTaskControl = new ProgressTaskControl(this);
			tasks.Add(taskName, progressTaskControl);
			flowLayoutPanel1.Controls.Add(progressTaskControl);
			((IProgressNotificationTask)progressTaskControl).BeginTask(taskName, taskText, totalWork, allowCancel);
			if (savedHeight == 0)
			{
				savedHeight = base.Height;
			}
			else
			{
				base.Height = savedHeight;
			}
			SetPosition();
		}
		else
		{
			SilentProgressNotificationTask silentProgressNotificationTask = new SilentProgressNotificationTask();
			tasks.Add(taskName, silentProgressNotificationTask);
			((IProgressNotificationTask)silentProgressNotificationTask).BeginTask(taskName, taskText, totalWork, allowCancel);
		}
	}

	private bool WasTaskStarted(string taskName)
	{
		return tasks.ContainsKey(taskName);
	}

	bool IProgressNotificationCenter.TaskStarted(string taskName)
	{
		return WasTaskStarted(taskName);
	}

	int IProgressNotificationCenter.GetWorkDone(string taskName)
	{
		if (tasks.ContainsKey(taskName))
		{
			return tasks[taskName].WorkDone;
		}
		return 0;
	}

	void IProgressNotificationCenter.SetTaskTextAndWork(string taskName, string taskText, int workValue)
	{
		if (tasks.TryGetValue(taskName, out var value))
		{
			value.TaskText = taskText;
			value.WorkDone = workValue;
			ThisRefresh();
		}
	}

	void IProgressNotificationCenter.SetWorkDone(string taskName, int value)
	{
		if (tasks.ContainsKey(taskName))
		{
			tasks[taskName].WorkDone = value;
			ThisRefresh();
		}
	}

	void IProgressNotificationCenter.SetTaskText(string taskName, string value)
	{
		if (tasks.ContainsKey(taskName))
		{
			tasks[taskName].TaskText = value;
			ThisRefresh();
		}
	}

	bool IProgressNotificationCenter.IsWorkDone(string taskName)
	{
		if (tasks.ContainsKey(taskName))
		{
			return tasks[taskName].IsWorkDone;
		}
		return false;
	}

	void IProgressNotificationCenter.IncreaseWorkDoneBy(string taskName, int value)
	{
		if (tasks.ContainsKey(taskName))
		{
			IProgressNotificationTask progressNotificationTask = tasks[taskName];
			if (!progressNotificationTask.IsWorkDone)
			{
				progressNotificationTask.WorkDone += value;
				ThisRefresh();
			}
		}
	}

	void IProgressNotificationCenter.DecreaseWorkDoneBy(string taskName, int value)
	{
		if (tasks.ContainsKey(taskName))
		{
			IProgressNotificationTask progressNotificationTask = tasks[taskName];
			if (!progressNotificationTask.IsWorkDone)
			{
				progressNotificationTask.WorkDone -= value;
				ThisRefresh();
			}
		}
	}

	private void TaskDone(string taskName)
	{
		if (tasks.ContainsKey(taskName))
		{
			tasks[taskName].Done();
			tasks.Remove(taskName);
			if (ShowNotifications)
			{
				SetPosition();
				Update();
				base.ParentForm.Update();
			}
		}
	}

	void IProgressNotificationCenter.Done(string taskName)
	{
		TaskDone(taskName);
	}

	bool IProgressNotificationCenter.GetIsCancelled(string taskName)
	{
		if (WasTaskStarted(taskName))
		{
			return tasks[taskName].IsCancelled;
		}
		return false;
	}

	void IProgressNotificationCenter.SetIsCancelled(string taskName, bool value)
	{
		if (WasTaskStarted(taskName))
		{
			tasks[taskName].IsCancelled = value;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
		base.SuspendLayout();
		this.flowLayoutPanel1.AutoSize = true;
		this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
		this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
		this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
		this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
		this.flowLayoutPanel1.Name = "flowLayoutPanel1";
		this.flowLayoutPanel1.Size = new System.Drawing.Size(0, 0);
		this.flowLayoutPanel1.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoSize = true;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
		this.BackColor = System.Drawing.Color.Transparent;
		base.Controls.Add(this.flowLayoutPanel1);
		this.DoubleBuffered = true;
		base.Margin = new System.Windows.Forms.Padding(0);
		base.Name = "ProgressTaskControlManager";
		base.Size = new System.Drawing.Size(0, 0);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
