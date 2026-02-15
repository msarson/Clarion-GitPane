using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class AddNewConfigurationDialog : Form
{
	private Predicate<string> checkNameValid;

	private IContainer components;

	private Button cancelButton;

	private Button okButton;

	private CheckBox createInAllCheckBox;

	private ComboBox copyFromComboBox;

	private Label label2;

	private TextBox nameTextBox;

	private Label label1;

	public bool CreateInAllProjects => createInAllCheckBox.Checked;

	public string CopyFrom
	{
		get
		{
			if (copyFromComboBox.SelectedIndex <= 0)
			{
				return null;
			}
			return copyFromComboBox.SelectedItem.ToString();
		}
	}

	public string NewName => nameTextBox.Text;

	public AddNewConfigurationDialog(bool solution, bool editPlatforms, IEnumerable<string> availableSourceItems, Predicate<string> checkNameValid)
	{
		this.checkNameValid = checkNameValid;
		InitializeComponent();
		foreach (Control control in base.Controls)
		{
			control.Text = StringParser.Parse(control.Text);
		}
		createInAllCheckBox.Visible = solution;
		nameTextBox.TextChanged += delegate
		{
			okButton.Enabled = nameTextBox.TextLength > 0;
		};
		copyFromComboBox.Items.Add("<Empty>");
		copyFromComboBox.Items.AddRange(Linq.ToArray(availableSourceItems));
		copyFromComboBox.SelectedIndex = 0;
		if (solution)
		{
			if (editPlatforms)
			{
				Text = "Add Solution Platform";
			}
			else
			{
				Text = "Add Solution Configuration";
			}
		}
		else if (editPlatforms)
		{
			Text = "Add Project Platform";
		}
		else
		{
			Text = "Add Project Configuration";
		}
	}

	private void OkButtonClick(object sender, EventArgs e)
	{
		if (checkNameValid(nameTextBox.Text))
		{
			base.DialogResult = DialogResult.OK;
			Close();
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
		this.nameTextBox = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.copyFromComboBox = new System.Windows.Forms.ComboBox();
		this.createInAllCheckBox = new System.Windows.Forms.CheckBox();
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.label1.Location = new System.Drawing.Point(12, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(237, 23);
		this.label1.TabIndex = 0;
		this.label1.Text = "${res:Dialog.NewProject.NameLabelText}";
		this.nameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.nameTextBox.Location = new System.Drawing.Point(12, 26);
		this.nameTextBox.Name = "nameTextBox";
		this.nameTextBox.Size = new System.Drawing.Size(237, 20);
		this.nameTextBox.TabIndex = 1;
		this.label2.Location = new System.Drawing.Point(12, 49);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(237, 23);
		this.label2.TabIndex = 2;
		this.label2.Text = "Copy &settings from:";
		this.copyFromComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.copyFromComboBox.FormattingEnabled = true;
		this.copyFromComboBox.Location = new System.Drawing.Point(12, 65);
		this.copyFromComboBox.Name = "copyFromComboBox";
		this.copyFromComboBox.Size = new System.Drawing.Size(237, 21);
		this.copyFromComboBox.TabIndex = 3;
		this.createInAllCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.createInAllCheckBox.Location = new System.Drawing.Point(12, 92);
		this.createInAllCheckBox.Name = "createInAllCheckBox";
		this.createInAllCheckBox.Size = new System.Drawing.Size(237, 24);
		this.createInAllCheckBox.TabIndex = 4;
		this.createInAllCheckBox.Text = "&Create this configuration in all projects";
		this.createInAllCheckBox.UseVisualStyleBackColor = true;
		this.okButton.Enabled = false;
		this.okButton.Location = new System.Drawing.Point(93, 127);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(75, 23);
		this.okButton.TabIndex = 5;
		this.okButton.Text = "${res:Global.OKButtonText}";
		this.okButton.UseVisualStyleBackColor = true;
		this.okButton.Click += new System.EventHandler(OkButtonClick);
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(174, 127);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(75, 23);
		this.cancelButton.TabIndex = 6;
		this.cancelButton.Text = "${res:Global.CancelButtonText}";
		this.cancelButton.UseVisualStyleBackColor = true;
		base.AcceptButton = this.okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(263, 162);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.createInAllCheckBox);
		base.Controls.Add(this.copyFromComboBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.nameTextBox);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "AddNewConfigurationDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "AddNewConfigurationDialog";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
