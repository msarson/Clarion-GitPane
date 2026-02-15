using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ProgressTaskControl : UserControl, IProgressNotificationTask
{
	private ProgressTaskControlManager _Manager;

	private int totalWork;

	private int workDone;

	private string taskName;

	private string taskText;

	private bool allowCancel;

	private DateTime nextUpdate = DateTime.MinValue;

	private bool _IsCancelled;

	private IContainer components;

	private ProgressBar statusProgressBar;

	private Label messageLabel;

	private Button cancelTaskButton;

	private SplitContainer splitContainer1;

	private ProgressTaskControlManager Manager => _Manager;

	int IProgressNotificationTask.WorkDone
	{
		get
		{
			return workDone;
		}
		set
		{
			if (workDone != value && !_IsCancelled)
			{
				workDone = value;
				SetWorkDone();
			}
		}
	}

	private bool HasWorkDone
	{
		get
		{
			if (workDone > 0)
			{
				return workDone >= statusProgressBar.Maximum;
			}
			return false;
		}
	}

	bool IProgressNotificationTask.IsWorkDone => HasWorkDone;

	string IProgressNotificationTask.TaskText
	{
		get
		{
			return taskText;
		}
		set
		{
			if (taskText != value)
			{
				taskText = value;
				SetTaskText();
				if (!base.Visible && !string.IsNullOrEmpty(taskText))
				{
					base.Visible = true;
					Refresh();
					Manager.SetPosition();
				}
			}
		}
	}

	string IProgressNotificationTask.TaskName
	{
		get
		{
			return taskName;
		}
		set
		{
			taskName = value;
		}
	}

	bool IProgressNotificationTask.IsCancelled
	{
		get
		{
			return _IsCancelled;
		}
		set
		{
			_IsCancelled = value;
		}
	}

	public ProgressTaskControl()
	{
		InitializeComponent();
		base.Visible = false;
	}

	public ProgressTaskControl(ProgressTaskControlManager manager)
		: this()
	{
		_Manager = manager;
		cancelTaskButton.Image = IconService.GetBitmap("Icons.24x24.CancelTaskIcon");
	}

	void IProgressNotificationTask.Show()
	{
		base.Visible = true;
		Show();
	}

	void IProgressNotificationTask.BeginTask(string name, int totalWork, bool allowCancel)
	{
		DoBeginTask(taskName, null, totalWork, allowCancel);
	}

	void IProgressNotificationTask.BeginTask(string taskName, string taskText, int totalWork, bool allowCancel)
	{
		DoBeginTask(taskName, taskText, totalWork, allowCancel);
	}

	private void DoBeginTask(string taskName, string taskText, int totalWork, bool allowCancel)
	{
		workDone = 0;
		this.taskName = taskName;
		this.taskText = taskText;
		base.Visible = false;
		if (taskText == null)
		{
			this.taskText = this.taskName;
			messageLabel.Text = this.taskName;
		}
		this.totalWork = totalWork;
		this.allowCancel = allowCancel;
		statusProgressBar.Value = 0;
		if (totalWork > 0)
		{
			statusProgressBar.Maximum = totalWork;
		}
		else
		{
			statusProgressBar.Maximum = 0;
		}
		if (!_Manager.Visible)
		{
			_Manager.Visible = true;
		}
		if (allowCancel)
		{
			splitContainer1.Panel1Collapsed = false;
		}
		else
		{
			splitContainer1.Panel1Collapsed = true;
		}
		if (totalWork == 0)
		{
			statusProgressBar.Visible = false;
			messageLabel.Dock = DockStyle.Fill;
			SetWorkDone();
			Manager.Refresh();
		}
	}

	private void SetWorkDone()
	{
		if (workDone > statusProgressBar.Maximum)
		{
			workDone = statusProgressBar.Maximum;
		}
		MakeVisible();
		statusProgressBar.Value = workDone;
		Refresh();
	}

	public bool GetVisible()
	{
		return base.Visible;
	}

	private void MakeVisible()
	{
		if (!base.Visible)
		{
			statusProgressBar.Value = 0;
			statusProgressBar.Maximum = totalWork;
			SetTaskText();
			if (!string.IsNullOrEmpty(taskText))
			{
				base.Visible = true;
				Refresh();
				Manager.SetPosition();
			}
		}
	}

	private void SetTaskText()
	{
		if (DateTime.Now > nextUpdate)
		{
			nextUpdate = DateTime.Now.AddSeconds(1.0);
			messageLabel.Text = StringParser.Parse(taskText);
			Refresh();
		}
	}

	void IProgressNotificationTask.Done()
	{
		workDone = 0;
		taskName = null;
		statusProgressBar.Visible = false;
		statusProgressBar.Value = 0;
		base.Visible = false;
		if (Manager != null)
		{
			Manager.Controls.Remove(this);
			Manager.SetPosition();
			_Manager = null;
			Dispose();
		}
	}

	private void OnCancelTaskButton_Click(object sender, EventArgs e)
	{
		if (allowCancel && !_IsCancelled)
		{
			_IsCancelled = true;
			cancelTaskButton.Enabled = false;
			messageLabel.Enabled = false;
			statusProgressBar.Enabled = false;
		}
	}

	protected override void Dispose(bool disposing)
	{
		_Manager = null;
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.statusProgressBar = new System.Windows.Forms.ProgressBar();
		this.messageLabel = new System.Windows.Forms.Label();
		this.cancelTaskButton = new System.Windows.Forms.Button();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		base.SuspendLayout();
		this.statusProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.statusProgressBar.Location = new System.Drawing.Point(0, 24);
		this.statusProgressBar.Margin = new System.Windows.Forms.Padding(0);
		this.statusProgressBar.Name = "statusProgressBar";
		this.statusProgressBar.Size = new System.Drawing.Size(259, 12);
		this.statusProgressBar.TabIndex = 0;
		this.messageLabel.AutoEllipsis = true;
		this.messageLabel.Dock = System.Windows.Forms.DockStyle.Top;
		this.messageLabel.Location = new System.Drawing.Point(0, 0);
		this.messageLabel.Name = "messageLabel";
		this.messageLabel.Size = new System.Drawing.Size(259, 18);
		this.messageLabel.TabIndex = 1;
		this.messageLabel.Text = "Task";
		this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cancelTaskButton.Location = new System.Drawing.Point(0, 6);
		this.cancelTaskButton.Margin = new System.Windows.Forms.Padding(0);
		this.cancelTaskButton.Name = "cancelTaskButton";
		this.cancelTaskButton.Size = new System.Drawing.Size(24, 25);
		this.cancelTaskButton.TabIndex = 2;
		this.cancelTaskButton.UseVisualStyleBackColor = true;
		this.cancelTaskButton.Click += new System.EventHandler(OnCancelTaskButton_Click);
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(5, 5);
		this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.cancelTaskButton);
		this.splitContainer1.Panel1MinSize = 24;
		this.splitContainer1.Panel2.Controls.Add(this.statusProgressBar);
		this.splitContainer1.Panel2.Controls.Add(this.messageLabel);
		this.splitContainer1.Size = new System.Drawing.Size(293, 36);
		this.splitContainer1.SplitterDistance = 33;
		this.splitContainer1.SplitterWidth = 1;
		this.splitContainer1.TabIndex = 4;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Info;
		base.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		base.Controls.Add(this.splitContainer1);
		base.Margin = new System.Windows.Forms.Padding(0);
		this.MaximumSize = new System.Drawing.Size(305, 48);
		this.MinimumSize = new System.Drawing.Size(305, 48);
		base.Name = "ProgressTaskControl";
		base.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
		base.Size = new System.Drawing.Size(303, 46);
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
