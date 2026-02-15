using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Common.Controls;

public class WaitForm : TransparentForm
{
	private IContainer components;

	private ProgressDisk progressDisk1;

	private Timer timer1;

	public WaitForm()
	{
		InitializeComponent();
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		if (progressDisk1.Value == 100)
		{
			progressDisk1.Value = 0;
		}
		progressDisk1.Value += 1;
	}

	private void WaitForm_Load(object sender, EventArgs e)
	{
		timer1.Start();
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
		this.components = new System.ComponentModel.Container();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.progressDisk1 = new SoftVelocity.Common.Controls.ProgressDisk();
		base.SuspendLayout();
		this.timer1.Interval = 150;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		this.progressDisk1.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.progressDisk1.BackColor = System.Drawing.Color.Transparent;
		this.progressDisk1.BackGroundColor = System.Drawing.Color.Transparent;
		this.progressDisk1.BlockSize = SoftVelocity.Common.Controls.ProgressDisk.BlockSizeType.Medium;
		this.progressDisk1.Location = new System.Drawing.Point(-38, -37);
		this.progressDisk1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
		this.progressDisk1.Name = "progressDisk1";
		this.progressDisk1.Size = new System.Drawing.Size(79, 79);
		this.progressDisk1.SliceCount = 7;
		this.progressDisk1.SquareSize = 79;
		this.progressDisk1.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(0, 0);
		base.Controls.Add(this.progressDisk1);
		base.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
		base.Name = "WaitForm";
		this.Text = "WaitForm";
		base.Load += new System.EventHandler(WaitForm_Load);
		base.ResumeLayout(false);
	}
}
