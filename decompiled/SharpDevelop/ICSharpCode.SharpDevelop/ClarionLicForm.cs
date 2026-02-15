using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using SeriousBit.Licensing;

namespace ICSharpCode.SharpDevelop;

public class ClarionLicForm : Form
{
	private const string invalidKeyNameText = "*** Invalid key ***";

	private const string invalidKeySerialText = "*** Invalid key ***";

	private SerialsManager manager;

	private Properties prop;

	private int t = 4;

	private bool win32;

	private IContainer components;

	private Button validBtn;

	private TextBox snumText;

	private TextBox nameTxt;

	private Label label1;

	private Label label2;

	private Button buttonCancel;

	private Button buttonPaste;

	public ClarionLicForm()
	{
		InitializeComponent();
	}

	public ClarionLicForm(SerialsManager m, Properties p, bool win32)
	{
		this.win32 = win32;
		manager = m;
		prop = p;
		InitializeComponent();
		BackgroundImage = IconService.GetBitmap("Icons.LicenseImage");
		if (this.win32)
		{
			Text += " - Clarion For Windows";
		}
		else
		{
			Text += " - Clarion#";
		}
		string text = prop.Get("Name", "");
		if (!string.IsNullOrEmpty(text) && !text.Equals("The Serial number used to activate Clarion is invalid or expired", StringComparison.OrdinalIgnoreCase) && !text.Equals("*** Invalid key ***", StringComparison.OrdinalIgnoreCase))
		{
			nameTxt.Text = text;
			nameTxt.ReadOnly = true;
			snumText.Select();
		}
	}

	private void validBtn_Click(object sender, EventArgs e)
	{
		t--;
		try
		{
			snumText.Text = snumText.Text.Trim();
			if (manager.IsValid(snumText.Text))
			{
				_ = string.Empty;
				string empty = string.Empty;
				string info = manager.GetInfo(snumText.Text);
				string[] array = info.Split(new char[1] { ':' }, 3);
				_ = array[0];
				Convert.ToInt32(array[1]);
				empty = array[2];
				if (nameTxt.Text.Trim().ToUpper() == empty.Trim().ToUpper())
				{
					base.DialogResult = DialogResult.OK;
					prop.Set("Name", nameTxt.Text.Trim());
					if (win32)
					{
						prop.Set(ClarionLic.propSerialString, snumText.Text.Trim());
					}
					else
					{
						prop.Set(ClarionLic.propClarionSharpSerialString, snumText.Text.Trim());
					}
					Close();
				}
				else
				{
					nameTxt.Text = "*** Invalid key ***";
					snumText.Text = "*** Invalid key ***";
				}
			}
			else
			{
				nameTxt.Text = "*** Invalid key ***";
				snumText.Text = "*** Invalid key ***";
			}
			if (t < 1)
			{
				base.DialogResult = DialogResult.No;
				Close();
			}
		}
		catch (Exception)
		{
			nameTxt.Text = "*** Invalid key ***";
			snumText.Text = "*** Invalid key ***";
			if (t < 1)
			{
				base.DialogResult = DialogResult.No;
				Close();
			}
		}
	}

	private void buttonCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.No;
		Close();
	}

	private void buttonPaste_Click(object sender, EventArgs e)
	{
		try
		{
			string text = Clipboard.GetText();
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length > 2)
				{
					List<string> list = new List<string>(array);
					for (int num = array.Length - 1; num > -1; num--)
					{
						if (string.IsNullOrEmpty(list[num].Trim()))
						{
							list.RemoveAt(num);
						}
					}
					array = list.ToArray();
				}
				if (array.Length == 2)
				{
					array[0] = array[0].Trim();
					array[1] = array[1].Trim();
					if (array[0].StartsWith("User Name:") && array[1].StartsWith("Serial Number:"))
					{
						if (nameTxt.ReadOnly && array[0].Substring(10).Trim() != nameTxt.Text.Trim())
						{
							nameTxt.Text = "*** Invalid key ***";
							snumText.Text = "*** Invalid key ***";
						}
						else
						{
							nameTxt.Text = array[0].Substring(10).Trim();
							snumText.Text = array[1].Substring(14).Trim();
							validBtn.Select();
						}
						return;
					}
				}
			}
		}
		catch
		{
			nameTxt.Text = "*** Invalid key ***";
			snumText.Text = "*** Invalid key ***";
		}
		nameTxt.Text = "*** Invalid key ***";
		snumText.Text = "*** Invalid key ***";
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
		this.validBtn = new System.Windows.Forms.Button();
		this.snumText = new System.Windows.Forms.TextBox();
		this.nameTxt = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonPaste = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.validBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.validBtn.Location = new System.Drawing.Point(382, 356);
		this.validBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.validBtn.Name = "validBtn";
		this.validBtn.Size = new System.Drawing.Size(101, 44);
		this.validBtn.TabIndex = 4;
		this.validBtn.Text = "Validate";
		this.validBtn.UseVisualStyleBackColor = true;
		this.validBtn.Click += new System.EventHandler(validBtn_Click);
		this.snumText.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.snumText.Location = new System.Drawing.Point(71, 317);
		this.snumText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.snumText.Name = "snumText";
		this.snumText.Size = new System.Drawing.Size(540, 27);
		this.snumText.TabIndex = 3;
		this.nameTxt.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.nameTxt.Location = new System.Drawing.Point(71, 255);
		this.nameTxt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.nameTxt.Name = "nameTxt";
		this.nameTxt.Size = new System.Drawing.Size(540, 27);
		this.nameTxt.TabIndex = 1;
		this.label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.label1.AutoSize = true;
		this.label1.BackColor = System.Drawing.Color.Transparent;
		this.label1.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.Black;
		this.label1.Location = new System.Drawing.Point(71, 293);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(107, 20);
		this.label1.TabIndex = 2;
		this.label1.Text = "Serial Number:";
		this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.Transparent;
		this.label2.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.ForeColor = System.Drawing.Color.Black;
		this.label2.Location = new System.Drawing.Point(71, 229);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(85, 20);
		this.label2.TabIndex = 0;
		this.label2.Text = "User Name:";
		this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(513, 356);
		this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(99, 41);
		this.buttonCancel.TabIndex = 5;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
		this.buttonPaste.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.buttonPaste.Location = new System.Drawing.Point(249, 356);
		this.buttonPaste.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.buttonPaste.Name = "buttonPaste";
		this.buttonPaste.Size = new System.Drawing.Size(99, 41);
		this.buttonPaste.TabIndex = 6;
		this.buttonPaste.Text = "Paste";
		this.buttonPaste.UseVisualStyleBackColor = true;
		this.buttonPaste.Click += new System.EventHandler(buttonPaste_Click);
		base.AcceptButton = this.validBtn;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 20f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		base.CancelButton = this.buttonCancel;
		base.ClientSize = new System.Drawing.Size(623, 410);
		base.ControlBox = false;
		base.Controls.Add(this.buttonPaste);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.nameTxt);
		base.Controls.Add(this.snumText);
		base.Controls.Add(this.validBtn);
		this.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ClarionLicForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Registration Form";
		base.TopMost = true;
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
