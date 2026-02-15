using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Clarion.ASL;
using Clarion.GEN;
using SoftVelocity.Generator.Properties;

namespace SoftVelocity.Generator;

public class GeneratorMessage : Form, IMessage
{
	private int buttonClicked = 1;

	private bool button2Visible;

	private bool button3Visible;

	private IContainer components;

	private TableLayoutPanel outerTableLayoutPanel;

	private TableLayoutPanel buttonTableLayoutPanel;

	private Label messageLabel;

	private TextBox messageText;

	private Button button1;

	private Button button2;

	private PictureBox iconPictureBox;

	private Button button3;

	public GeneratorMessage()
	{
		InitializeComponent();
	}

	private void Button1Clicked(object sender, EventArgs e)
	{
		buttonClicked = 1;
		Close();
	}

	private void Button2Clicked(object sender, EventArgs e)
	{
		buttonClicked = 2;
		Close();
	}

	private void Button3Clicked(object sender, EventArgs e)
	{
		buttonClicked = 3;
		Close();
	}

	private int ButtonWidth(Button button, bool buttonVisible)
	{
		if (!buttonVisible)
		{
			return 0;
		}
		return button.Width;
	}

	private void InitializeTextBox()
	{
		messageText.Text = messageLabel.Text;
		messageText.Width = messageLabel.Width;
		messageLabel.Visible = false;
		messageLabel.Text = string.Empty;
		int num = messageText.GetLineFromCharIndex(messageText.TextLength) + 1;
		int num2 = messageText.Height - messageText.ClientSize.Height;
		messageText.Height = messageText.Font.Height * num + 3 + num2;
	}

	private void FormLoaded(object sender, EventArgs e)
	{
		int num = base.Width;
		base.Width = Math.Max(ButtonWidth(button1, buttonVisible: true) + ButtonWidth(button2, button2Visible) + ButtonWidth(button3, button3Visible) + 187, messageLabel.Width + 71);
		base.Left += (num - base.Width) / 2;
		InitializeTextBox();
	}

	public int Show(string message, string title, string[] buttons)
	{
		if (Win32Generator.CommandLineLogger != null)
		{
			Win32Generator.CommandLineLogger.Message(message);
			return buttons.Length;
		}
		string value = buttons[0];
		Text = title;
		messageLabel.Visible = true;
		messageLabel.Text = message;
		messageLabel.Refresh();
		if (buttons.Length < 3)
		{
			button2.Visible = false;
			button2Visible = false;
			if (buttons.Length == 1)
			{
				button3.Visible = false;
				button3Visible = false;
				if (string.IsNullOrEmpty(value))
				{
					value = "OK";
				}
			}
			else
			{
				button3.Text = buttons[1];
				button3.Click -= Button3Clicked;
				button3.Click += Button2Clicked;
			}
		}
		else
		{
			button2Visible = true;
			button3Visible = true;
			button3.Text = buttons[2];
			button2.Text = buttons[1];
		}
		button1.Text = value;
		ShowDialog();
		return buttonClicked;
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
		this.outerTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.buttonTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.button1 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.button3 = new System.Windows.Forms.Button();
		this.messageLabel = new System.Windows.Forms.Label();
		this.messageText = new System.Windows.Forms.TextBox();
		this.iconPictureBox = new System.Windows.Forms.PictureBox();
		this.outerTableLayoutPanel.SuspendLayout();
		this.buttonTableLayoutPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.iconPictureBox).BeginInit();
		base.SuspendLayout();
		this.outerTableLayoutPanel.AutoSize = true;
		this.outerTableLayoutPanel.ColumnCount = 2;
		this.outerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.outerTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.outerTableLayoutPanel.Controls.Add(this.buttonTableLayoutPanel, 1, 1);
		this.outerTableLayoutPanel.Controls.Add(this.messageText, 1, 0);
		this.outerTableLayoutPanel.Controls.Add(this.iconPictureBox, 0, 0);
		this.outerTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.outerTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.outerTableLayoutPanel.Name = "outerTableLayoutPanel";
		this.outerTableLayoutPanel.RowCount = 2;
		this.outerTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.outerTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.outerTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.outerTableLayoutPanel.Size = new System.Drawing.Size(921, 85);
		this.outerTableLayoutPanel.TabIndex = 0;
		this.buttonTableLayoutPanel.AutoSize = true;
		this.buttonTableLayoutPanel.ColumnCount = 3;
		this.buttonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.buttonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.buttonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00001f));
		this.buttonTableLayoutPanel.Controls.Add(this.button1, 0, 0);
		this.buttonTableLayoutPanel.Controls.Add(this.button2, 1, 0);
		this.buttonTableLayoutPanel.Controls.Add(this.button3, 2, 0);
		this.buttonTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonTableLayoutPanel.Location = new System.Drawing.Point(55, 55);
		this.buttonTableLayoutPanel.Name = "buttonTableLayoutPanel";
		this.buttonTableLayoutPanel.RowCount = 1;
		this.buttonTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.buttonTableLayoutPanel.Size = new System.Drawing.Size(863, 27);
		this.buttonTableLayoutPanel.TabIndex = 1;
		this.button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.button1.AutoSize = true;
		this.button1.Location = new System.Drawing.Point(278, 3);
		this.button1.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 21);
		this.button1.TabIndex = 0;
		this.button1.Text = "button1";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(Button1Clicked);
		this.button2.AutoSize = true;
		this.button2.Location = new System.Drawing.Point(393, 3);
		this.button2.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 21);
		this.button2.TabIndex = 1;
		this.button2.Text = "b2";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(Button2Clicked);
		this.button3.AutoSize = true;
		this.button3.Location = new System.Drawing.Point(508, 3);
		this.button3.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(75, 21);
		this.button3.TabIndex = 2;
		this.button3.Text = "button3";
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(Button3Clicked);
		this.messageLabel.AutoSize = true;
		this.messageLabel.Location = new System.Drawing.Point(55, 5);
		this.messageLabel.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
		this.messageLabel.Name = "messageLabel";
		this.messageLabel.Size = new System.Drawing.Size(125, 47);
		this.messageLabel.TabIndex = 2;
		this.messageLabel.Text = "Text\r\nspread over multiple lines\r\nto test things out";
		this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.messageText.AutoSize = false;
		this.messageText.Location = new System.Drawing.Point(55, 5);
		this.messageText.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
		this.messageText.Name = "messageText";
		this.messageText.Size = new System.Drawing.Size(125, 47);
		this.messageText.TabIndex = 2;
		this.messageText.Text = "Text\r\nspread over multiple lines\r\nto test things out";
		this.messageText.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
		this.messageText.ReadOnly = true;
		this.messageText.TabStop = false;
		this.messageText.Multiline = true;
		this.messageText.WordWrap = false;
		this.messageText.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.messageText.Dock = System.Windows.Forms.DockStyle.Top;
		this.iconPictureBox.Image = SoftVelocity.Generator.Properties.Resources.ASTERISK;
		this.iconPictureBox.Location = new System.Drawing.Point(10, 10);
		this.iconPictureBox.Margin = new System.Windows.Forms.Padding(10, 10, 10, 10);
		this.iconPictureBox.Name = "iconPictureBox";
		this.iconPictureBox.Size = new System.Drawing.Size(32, 32);
		this.iconPictureBox.TabIndex = 3;
		this.iconPictureBox.TabStop = false;
		base.AcceptButton = this.button1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		this.AutoSize = true;
		base.ClientSize = new System.Drawing.Size(921, 85);
		base.ControlBox = false;
		base.Controls.Add(this.outerTableLayoutPanel);
		base.Controls.Add(this.messageLabel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "GeneratorMessage";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "GenMessage";
		base.Load += new System.EventHandler(FormLoaded);
		this.outerTableLayoutPanel.ResumeLayout(false);
		this.outerTableLayoutPanel.PerformLayout();
		this.buttonTableLayoutPanel.ResumeLayout(false);
		this.buttonTableLayoutPanel.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.iconPictureBox).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
