using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Generator.UI;

public class NewAppDialog : Form
{
	private bool _UseWizard;

	private IContainer components;

	private Button buttonOk;

	private Button buttonCancel;

	private CheckBox checkBoxUseWizard;

	public bool UseWizard => _UseWizard;

	public NewAppDialog()
	{
		InitializeComponent();
	}

	private void buttonOk_Click(object sender, EventArgs e)
	{
		if (checkBoxUseWizard.Checked)
		{
			_UseWizard = true;
		}
		else
		{
			_UseWizard = false;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void buttonCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.buttonOk = new System.Windows.Forms.Button();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.checkBoxUseWizard = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.buttonOk.Location = new System.Drawing.Point(134, 128);
		this.buttonOk.Name = "buttonOk";
		this.buttonOk.Size = new System.Drawing.Size(75, 23);
		this.buttonOk.TabIndex = 0;
		this.buttonOk.Text = "Ok";
		this.buttonOk.UseVisualStyleBackColor = true;
		this.buttonOk.Click += new System.EventHandler(buttonOk_Click);
		this.buttonCancel.Location = new System.Drawing.Point(247, 128);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 1;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
		this.checkBoxUseWizard.AutoSize = true;
		this.checkBoxUseWizard.Location = new System.Drawing.Point(134, 39);
		this.checkBoxUseWizard.Name = "checkBoxUseWizard";
		this.checkBoxUseWizard.Size = new System.Drawing.Size(81, 17);
		this.checkBoxUseWizard.TabIndex = 2;
		this.checkBoxUseWizard.Text = "Use Wizard";
		this.checkBoxUseWizard.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(391, 163);
		base.ControlBox = false;
		base.Controls.Add(this.checkBoxUseWizard);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOk);
		base.Name = "NewAppDialog";
		this.Text = "Form1";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
