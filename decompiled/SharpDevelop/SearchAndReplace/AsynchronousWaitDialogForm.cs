using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace SearchAndReplace;

internal sealed class AsynchronousWaitDialogForm : Form
{
	private IContainer components;

	internal Button cancelButton;

	internal ProgressBar progressBar;

	internal Label taskLabel;

	internal AsynchronousWaitDialogForm()
	{
		InitializeComponent();
		cancelButton.Text = ResourceService.GetString("Global.CancelButtonText");
	}

	private void CancelButtonClick(object sender, EventArgs e)
	{
		cancelButton.Enabled = false;
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
		this.taskLabel = new System.Windows.Forms.Label();
		this.progressBar = new System.Windows.Forms.ProgressBar();
		this.cancelButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.taskLabel.Location = new System.Drawing.Point(12, 9);
		this.taskLabel.Name = "taskLabel";
		this.taskLabel.Size = new System.Drawing.Size(311, 46);
		this.taskLabel.TabIndex = 0;
		this.taskLabel.Text = "Please wait...";
		this.taskLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.progressBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.progressBar.Location = new System.Drawing.Point(12, 58);
		this.progressBar.Name = "progressBar";
		this.progressBar.Size = new System.Drawing.Size(235, 22);
		this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
		this.progressBar.TabIndex = 1;
		this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(253, 58);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(75, 23);
		this.cancelButton.TabIndex = 2;
		this.cancelButton.Text = "button1";
		this.cancelButton.UseVisualStyleBackColor = true;
		this.cancelButton.Click += new System.EventHandler(CancelButtonClick);
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(336, 87);
		base.ControlBox = false;
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.progressBar);
		base.Controls.Add(this.taskLabel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "AsynchronousWaitDialogForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "AsynchronousWaitDialog";
		base.ResumeLayout(false);
	}
}
