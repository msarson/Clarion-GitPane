using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Common.Dialogs;

public class BlockIndentDialog : Form
{
	private IContainer components;

	private Button cancelButton;

	private Button okButton;

	private NumericUpDown indentUpDown;

	private RadioButton leftRadio;

	private RadioButton rightRadio;

	private Label label1;

	private GroupBox groupBox1;

	public int IndentValue => decimal.ToInt32(indentUpDown.Value);

	public BlockIndentDialog(int indent)
	{
		InitializeComponent();
		indentUpDown.Value = indent;
		if (indent >= 0)
		{
			rightRadio.Checked = true;
		}
		else
		{
			leftRadio.Checked = true;
		}
		indentUpDown.Select(0, indentUpDown.Text.Length);
	}

	private void leftRadio_CheckedChanged(object sender, EventArgs e)
	{
		if (leftRadio.Checked)
		{
			rightRadio.Checked = false;
			if (indentUpDown.Value > 0m)
			{
				indentUpDown.Value *= -1m;
			}
		}
	}

	private void rightRadio_CheckedChanged(object sender, EventArgs e)
	{
		if (rightRadio.Checked)
		{
			leftRadio.Checked = false;
			if (indentUpDown.Value < 0m)
			{
				indentUpDown.Value *= -1m;
			}
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
		this.label1 = new System.Windows.Forms.Label();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.rightRadio = new System.Windows.Forms.RadioButton();
		this.leftRadio = new System.Windows.Forms.RadioButton();
		this.cancelButton = new System.Windows.Forms.Button();
		this.okButton = new System.Windows.Forms.Button();
		this.indentUpDown = new System.Windows.Forms.NumericUpDown();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.indentUpDown).BeginInit();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 14);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(40, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "&Indent:";
		this.groupBox1.Controls.Add(this.rightRadio);
		this.groupBox1.Controls.Add(this.leftRadio);
		this.groupBox1.Location = new System.Drawing.Point(15, 37);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(115, 44);
		this.groupBox1.TabIndex = 2;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Direction";
		this.rightRadio.AutoSize = true;
		this.rightRadio.Location = new System.Drawing.Point(65, 17);
		this.rightRadio.Name = "rightRadio";
		this.rightRadio.Size = new System.Drawing.Size(37, 17);
		this.rightRadio.TabIndex = 1;
		this.rightRadio.TabStop = true;
		this.rightRadio.Text = "&>>";
		this.rightRadio.UseVisualStyleBackColor = true;
		this.rightRadio.CheckedChanged += new System.EventHandler(rightRadio_CheckedChanged);
		this.leftRadio.AutoSize = true;
		this.leftRadio.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.leftRadio.Location = new System.Drawing.Point(12, 17);
		this.leftRadio.Name = "leftRadio";
		this.leftRadio.Size = new System.Drawing.Size(37, 17);
		this.leftRadio.TabIndex = 0;
		this.leftRadio.TabStop = true;
		this.leftRadio.Text = "&<<";
		this.leftRadio.UseVisualStyleBackColor = true;
		this.leftRadio.CheckedChanged += new System.EventHandler(leftRadio_CheckedChanged);
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(143, 41);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(70, 23);
		this.cancelButton.TabIndex = 4;
		this.cancelButton.Text = "&Cancel";
		this.cancelButton.UseVisualStyleBackColor = true;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Location = new System.Drawing.Point(143, 12);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(70, 23);
		this.okButton.TabIndex = 3;
		this.okButton.Text = "&Ok";
		this.okButton.UseVisualStyleBackColor = true;
		this.indentUpDown.Location = new System.Drawing.Point(67, 12);
		this.indentUpDown.Minimum = new decimal(new int[4] { 100, 0, 0, -2147483648 });
		this.indentUpDown.Name = "indentUpDown";
		this.indentUpDown.Size = new System.Drawing.Size(62, 20);
		this.indentUpDown.TabIndex = 1;
		base.AcceptButton = this.okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(226, 94);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.indentUpDown);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.cancelButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "BlockIndentDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Block Indent";
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.indentUpDown).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
