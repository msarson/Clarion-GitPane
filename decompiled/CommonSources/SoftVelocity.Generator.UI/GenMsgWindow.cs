using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Generator.UI;

public class GenMsgWindow : Form
{
	private bool _EnableCancel;

	private IContainer components;

	private Button GenCancelButton;

	private PictureBox pictureBox1;

	private Label GenMsgLine1;

	private Label GenMsgLine2;

	private Label GenMsgLine3;

	private Timer timer1;

	public bool EnableCancel
	{
		get
		{
			return _EnableCancel;
		}
		set
		{
			_EnableCancel = value;
			if (_EnableCancel)
			{
				base.StartPosition = FormStartPosition.WindowsDefaultLocation;
				GenCancelButton.Visible = true;
				base.ClientSize = new Size(356, 89);
				base.ControlBox = true;
			}
			else
			{
				base.StartPosition = FormStartPosition.CenterScreen;
				base.ClientSize = new Size(356, 60);
				GenCancelButton.Visible = false;
				base.ControlBox = false;
			}
		}
	}

	public event EventHandler Canceled;

	public GenMsgWindow()
	{
		InitializeComponent();
		GenMsgLine1.Text = "";
		GenMsgLine2.Text = "";
		GenMsgLine3.Text = "";
	}

	private void GenMsgWindow_Load(object sender, EventArgs e)
	{
	}

	public void SetTitle(string caption, bool debugmode)
	{
		if (debugmode)
		{
			Text = caption + " (DEBUG)";
		}
		else
		{
			Text = caption;
		}
	}

	public void SetText(string NewText, int WhichLine)
	{
		switch (WhichLine)
		{
		case 1:
			GenMsgLine1.Text = NewText;
			break;
		case 2:
			GenMsgLine2.Text = NewText;
			break;
		case 3:
			GenMsgLine3.Text = NewText;
			break;
		}
	}

	private void GenCancelButton_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void GenMsgWindow_FormClosed(object sender, FormClosedEventArgs e)
	{
		if (this.Canceled != null && _EnableCancel)
		{
			this.Canceled(null, null);
		}
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		Application.DoEvents();
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
		this.GenCancelButton = new System.Windows.Forms.Button();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.GenMsgLine1 = new System.Windows.Forms.Label();
		this.GenMsgLine2 = new System.Windows.Forms.Label();
		this.GenMsgLine3 = new System.Windows.Forms.Label();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.GenCancelButton.Location = new System.Drawing.Point(141, 63);
		this.GenCancelButton.Name = "GenCancelButton";
		this.GenCancelButton.Size = new System.Drawing.Size(75, 23);
		this.GenCancelButton.TabIndex = 0;
		this.GenCancelButton.Text = "Cancel";
		this.GenCancelButton.UseVisualStyleBackColor = true;
		this.GenCancelButton.Click += new System.EventHandler(GenCancelButton_Click);
		this.pictureBox1.Location = new System.Drawing.Point(12, 11);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(32, 32);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
		this.pictureBox1.TabIndex = 1;
		this.pictureBox1.TabStop = false;
		this.GenMsgLine1.AutoSize = true;
		this.GenMsgLine1.Location = new System.Drawing.Point(52, 6);
		this.GenMsgLine1.Name = "GenMsgLine1";
		this.GenMsgLine1.Size = new System.Drawing.Size(0, 13);
		this.GenMsgLine1.TabIndex = 2;
		this.GenMsgLine2.AutoSize = true;
		this.GenMsgLine2.Location = new System.Drawing.Point(52, 23);
		this.GenMsgLine2.Name = "GenMsgLine2";
		this.GenMsgLine2.Size = new System.Drawing.Size(0, 13);
		this.GenMsgLine2.TabIndex = 3;
		this.GenMsgLine3.AutoSize = true;
		this.GenMsgLine3.Location = new System.Drawing.Point(52, 42);
		this.GenMsgLine3.Name = "GenMsgLine3";
		this.GenMsgLine3.Size = new System.Drawing.Size(0, 13);
		this.GenMsgLine3.TabIndex = 4;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(356, 89);
		base.ControlBox = false;
		base.Controls.Add(this.GenMsgLine3);
		base.Controls.Add(this.GenMsgLine2);
		base.Controls.Add(this.GenMsgLine1);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.GenCancelButton);
		this.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "GenMsgWindow";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Source Generation";
		base.TopMost = true;
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(GenMsgWindow_FormClosed);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
