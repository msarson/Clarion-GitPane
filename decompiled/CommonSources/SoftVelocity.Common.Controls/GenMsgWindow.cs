using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CommonSources.Properties;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.Controls;

public class GenMsgWindow : TransparentForm
{
	private static GenMsgWindow instance;

	private static bool isOpen;

	private static bool isCancelled;

	private bool canCancel;

	private Size PrevSize;

	private IContainer components;

	private PictureBox pictureBox1;

	private Label GenMsgLine3;

	private Label GenMsgLine2;

	private Label GenMsgLine1;

	private Button GenCancelButton;

	private Grouper genBox;

	private Button GenOkButton;

	private bool EnableCancel
	{
		get
		{
			return canCancel;
		}
		set
		{
			canCancel = value;
			if (canCancel)
			{
				GenCancelButton.Visible = true;
				genBox.Size = new Size(PrevSize.Width, PrevSize.Height);
				ReShow();
			}
			else
			{
				base.Visible = false;
				PrevSize = new Size(genBox.Size.Width, genBox.Size.Height);
				genBox.Size = new Size(genBox.Size.Width, GenCancelButton.Location.Y);
				GenCancelButton.Visible = false;
			}
		}
	}

	public GenMsgWindow()
	{
		InitializeComponent();
		base.Opacity = 1.0;
		GenMsgLine1.Text = "";
		GenMsgLine2.Text = "";
		GenMsgLine3.Text = "";
		PrevSize = new Size(genBox.Size.Width, genBox.Size.Height);
		EnableCancel = false;
		isCancelled = false;
		SetStyle(ControlStyles.DoubleBuffer, value: false);
	}

	private void ShowStatusBar(string line1, string line2, string line3)
	{
		StringBuilder stringBuilder = new StringBuilder(genBox.GroupTitle);
		if (!string.IsNullOrEmpty(line1))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(line1);
		}
		if (!string.IsNullOrEmpty(line2))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(line2);
		}
		if (!string.IsNullOrEmpty(line3))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(line3);
		}
		StatusBarService.SetMessage(stringBuilder.ToString(), false, false);
	}

	private void ShowDialog(Form owner, string caption, string line1, string line2, string line3)
	{
		if (canCancel)
		{
			base.MainFrame = owner;
			genBox.GroupTitle = caption;
			GenMsgLine1.Text = line1;
			GenMsgLine2.Text = line2;
			GenMsgLine3.Text = line3;
			Show();
		}
		else
		{
			ShowStatusBar(line1, line2, line3);
		}
	}

	private void GenCancelButton_Click(object sender, EventArgs e)
	{
		if (canCancel)
		{
			isCancelled = true;
		}
	}

	private void GenOkButton_Click(object sender, EventArgs e)
	{
	}

	private void SetText(string NewText, int WhichLine)
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
		if (!canCancel)
		{
			ShowStatusBar(GenMsgLine1.Text, GenMsgLine2.Text, GenMsgLine3.Text);
		}
	}

	private void SetTitle(string caption, bool debugmode)
	{
		if (debugmode)
		{
			genBox.GroupTitle = caption + " (debug)";
		}
		else
		{
			genBox.GroupTitle = caption;
		}
		if (!canCancel)
		{
			ShowStatusBar(null, null, null);
		}
	}

	public static void StartGenMsg(bool enableCancel)
	{
		if (isOpen)
		{
			return;
		}
		if (instance == null)
		{
			instance = new GenMsgWindow();
			if (enableCancel)
			{
				instance.MainFrame = WorkbenchSingleton.MainForm;
				instance.Show();
			}
		}
		else
		{
			instance.EnableCancel = enableCancel;
		}
		isOpen = true;
	}

	public static void SetGenMsgText(string newText, int whichLine)
	{
		StartGenMsg(enableCancel: false);
		instance.SetText(newText, whichLine);
	}

	public static void SetGenMsgTitle(string caption, bool debugmode)
	{
		StartGenMsg(enableCancel: false);
		instance.SetTitle(caption, debugmode);
	}

	public static void CloseGenMsgWin()
	{
		if (isOpen)
		{
			instance.Visible = false;
			instance.genBox.GroupTitle = "";
			instance.GenMsgLine1.Text = "";
			instance.GenMsgLine2.Text = "";
			instance.GenMsgLine3.Text = "";
			isOpen = false;
		}
		StatusBarService.SetMessage(string.Empty, false, false);
	}

	public static void HideGenMsgWin(bool on)
	{
		if (isOpen)
		{
			instance.Visible = !on;
		}
	}

	public static bool CheckGenCancel()
	{
		return isCancelled;
	}

	public static void GenMsgShow(string caption, string line1, string line2, string line3)
	{
		GenMsgShow(WorkbenchSingleton.MainForm, caption, line1, line2, line3);
	}

	public static void GenMsgShow(Form owner, string caption, string line1, string line2, string line3)
	{
		if (!isOpen)
		{
			instance = new GenMsgWindow();
			instance.EnableCancel = false;
			instance.ShowDialog(owner, caption, line1, line2, line3);
			isOpen = true;
		}
		else if (instance.canCancel)
		{
			SetGenMsgTitle(caption, debugmode: false);
			SetGenMsgText(line1, 1);
			SetGenMsgText(line1, 2);
			SetGenMsgText(line1, 3);
		}
		else
		{
			instance.ShowStatusBar(line1, line2, line3);
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
		this.GenCancelButton = new System.Windows.Forms.Button();
		this.GenMsgLine3 = new System.Windows.Forms.Label();
		this.GenMsgLine2 = new System.Windows.Forms.Label();
		this.GenMsgLine1 = new System.Windows.Forms.Label();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.genBox = new SoftVelocity.Common.Controls.Grouper();
		this.GenOkButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		this.genBox.SuspendLayout();
		base.SuspendLayout();
		this.GenCancelButton.BackColor = System.Drawing.Color.Gainsboro;
		this.GenCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.GenCancelButton.ForeColor = System.Drawing.Color.Black;
		this.GenCancelButton.Location = new System.Drawing.Point(263, 94);
		this.GenCancelButton.Name = "GenCancelButton";
		this.GenCancelButton.Size = new System.Drawing.Size(75, 23);
		this.GenCancelButton.TabIndex = 4;
		this.GenCancelButton.Text = "Cancel";
		this.GenCancelButton.UseVisualStyleBackColor = false;
		this.GenCancelButton.Click += new System.EventHandler(GenCancelButton_Click);
		this.GenMsgLine3.AutoSize = true;
		this.GenMsgLine3.BackColor = System.Drawing.Color.White;
		this.GenMsgLine3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.GenMsgLine3.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.GenMsgLine3.Location = new System.Drawing.Point(48, 71);
		this.GenMsgLine3.Name = "GenMsgLine3";
		this.GenMsgLine3.Size = new System.Drawing.Size(84, 13);
		this.GenMsgLine3.TabIndex = 3;
		this.GenMsgLine3.Text = "GenMsgLine3";
		this.GenMsgLine2.AutoSize = true;
		this.GenMsgLine2.BackColor = System.Drawing.Color.White;
		this.GenMsgLine2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.GenMsgLine2.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.GenMsgLine2.Location = new System.Drawing.Point(48, 51);
		this.GenMsgLine2.Name = "GenMsgLine2";
		this.GenMsgLine2.Size = new System.Drawing.Size(84, 13);
		this.GenMsgLine2.TabIndex = 2;
		this.GenMsgLine2.Text = "GenMsgLine2";
		this.GenMsgLine1.AutoSize = true;
		this.GenMsgLine1.BackColor = System.Drawing.Color.White;
		this.GenMsgLine1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.GenMsgLine1.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.GenMsgLine1.Location = new System.Drawing.Point(48, 31);
		this.GenMsgLine1.Name = "GenMsgLine1";
		this.GenMsgLine1.Size = new System.Drawing.Size(84, 13);
		this.GenMsgLine1.TabIndex = 1;
		this.GenMsgLine1.Text = "GenMsgLine1";
		this.pictureBox1.BackColor = System.Drawing.Color.White;
		this.pictureBox1.Image = CommonSources.Properties.Resources.ASTERISK;
		this.pictureBox1.Location = new System.Drawing.Point(9, 41);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(32, 32);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
		this.pictureBox1.TabIndex = 0;
		this.pictureBox1.TabStop = false;
		this.genBox.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.genBox.AutoSize = true;
		this.genBox.BackgroundColor = System.Drawing.Color.White;
		this.genBox.BackgroundGradientColor = System.Drawing.Color.White;
		this.genBox.BackgroundGradientMode = SoftVelocity.Common.Controls.Grouper.GroupBoxGradientMode.None;
		this.genBox.BorderColor = System.Drawing.Color.Black;
		this.genBox.BorderThickness = 1f;
		this.genBox.Controls.Add(this.GenOkButton);
		this.genBox.Controls.Add(this.GenCancelButton);
		this.genBox.Controls.Add(this.GenMsgLine2);
		this.genBox.Controls.Add(this.pictureBox1);
		this.genBox.Controls.Add(this.GenMsgLine1);
		this.genBox.Controls.Add(this.GenMsgLine3);
		this.genBox.CustomGroupBoxColor = System.Drawing.Color.DodgerBlue;
		this.genBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.genBox.ForeColor = System.Drawing.Color.White;
		this.genBox.GroupImage = null;
		this.genBox.GroupTitle = "Title vbvc bvcxb fdgfd dfg dfg dfg dfg dfgdfg dfg dfg dfgd gffdg fdg fdg dg fdg df gdfg dfgvcb .vbvcbvbvbvcxbvcb";
		this.genBox.Location = new System.Drawing.Point(-171, -62);
		this.genBox.Name = "genBox";
		this.genBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.genBox.PaintGroupBox = true;
		this.genBox.RoundCorners = 10;
		this.genBox.ShadowColor = System.Drawing.Color.DarkGray;
		this.genBox.ShadowControl = false;
		this.genBox.ShadowThickness = 5;
		this.genBox.Size = new System.Drawing.Size(345, 124);
		this.genBox.TabIndex = 9;
		this.GenOkButton.BackColor = System.Drawing.Color.Gainsboro;
		this.GenOkButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.GenOkButton.ForeColor = System.Drawing.Color.Black;
		this.GenOkButton.Location = new System.Drawing.Point(182, 94);
		this.GenOkButton.Name = "GenOkButton";
		this.GenOkButton.Size = new System.Drawing.Size(75, 23);
		this.GenOkButton.TabIndex = 5;
		this.GenOkButton.Text = "Ok";
		this.GenOkButton.UseVisualStyleBackColor = false;
		this.GenOkButton.Visible = false;
		this.GenOkButton.Click += new System.EventHandler(GenOkButton_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoSize = true;
		this.BackColor = System.Drawing.Color.Fuchsia;
		base.CausesValidation = false;
		base.ClientSize = new System.Drawing.Size(0, 0);
		base.Controls.Add(this.genBox);
		this.ForeColor = System.Drawing.Color.Transparent;
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.Name = "GenMsgWindow";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "InfoForm";
		base.TransparencyKey = System.Drawing.Color.Fuchsia;
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		this.genBox.ResumeLayout(false);
		this.genBox.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
