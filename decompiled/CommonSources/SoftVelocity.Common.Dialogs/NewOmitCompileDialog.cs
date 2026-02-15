using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Common.Dialogs;

public class NewOmitCompileDialog : Form
{
	private IContainer components;

	private Label terminatorLabel;

	private TextBox terminatorEdit;

	private Label expressionLabel;

	private TextBox expressionEdit;

	private Button cancelButton;

	private Button okButton;

	public string Terminator => terminatorEdit.Text;

	public string Expression => expressionEdit.Text;

	public NewOmitCompileDialog(string command)
	{
		InitializeComponent();
		Text += command;
		terminatorEdit.Text = "**END**";
		terminatorEdit.Focus();
		terminatorEdit.SelectAll();
	}

	private void terminatorEdit_TextChanged(object sender, EventArgs e)
	{
		if (terminatorEdit.Text.Length == 0)
		{
			okButton.Enabled = false;
		}
		else
		{
			okButton.Enabled = true;
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
		this.terminatorLabel = new System.Windows.Forms.Label();
		this.terminatorEdit = new System.Windows.Forms.TextBox();
		this.expressionLabel = new System.Windows.Forms.Label();
		this.expressionEdit = new System.Windows.Forms.TextBox();
		this.cancelButton = new System.Windows.Forms.Button();
		this.okButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.terminatorLabel.Location = new System.Drawing.Point(8, 12);
		this.terminatorLabel.Name = "terminatorLabel";
		this.terminatorLabel.Size = new System.Drawing.Size(104, 23);
		this.terminatorLabel.TabIndex = 0;
		this.terminatorLabel.Text = "Terminator:";
		this.terminatorEdit.Location = new System.Drawing.Point(116, 12);
		this.terminatorEdit.Name = "terminatorEdit";
		this.terminatorEdit.Size = new System.Drawing.Size(260, 22);
		this.terminatorEdit.TabIndex = 1;
		this.terminatorEdit.TextChanged += new System.EventHandler(terminatorEdit_TextChanged);
		this.expressionLabel.Location = new System.Drawing.Point(8, 40);
		this.expressionLabel.Name = "expressionLabel";
		this.expressionLabel.Size = new System.Drawing.Size(104, 23);
		this.expressionLabel.TabIndex = 2;
		this.expressionLabel.Text = "Expression:";
		this.expressionEdit.Location = new System.Drawing.Point(116, 40);
		this.expressionEdit.Name = "expressionEdit";
		this.expressionEdit.Size = new System.Drawing.Size(260, 22);
		this.expressionEdit.TabIndex = 3;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(292, 80);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(84, 28);
		this.cancelButton.TabIndex = 5;
		this.cancelButton.Text = "Cancel";
		this.cancelButton.UseVisualStyleBackColor = true;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Location = new System.Drawing.Point(192, 80);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(84, 28);
		this.okButton.TabIndex = 4;
		this.okButton.Text = "Ok";
		this.okButton.UseVisualStyleBackColor = true;
		base.AcceptButton = this.okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(388, 119);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.expressionEdit);
		base.Controls.Add(this.expressionLabel);
		base.Controls.Add(this.terminatorEdit);
		base.Controls.Add(this.terminatorLabel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "NewOmitCompileDialog";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Create new ";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
