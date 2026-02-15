using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Clarion.Core.Redirection;

internal class MacroExceptionDialog : Form
{
	private IContainer components;

	private TableLayoutPanel pageTableLayoutPanel;

	private Label errorLabel;

	private Label fileLabel;

	private Label lineLabel;

	private Label macroLabel;

	private Label causeLabel;

	private ListView macrosListView;

	private ColumnHeader macrosColumnHeader;

	private TableLayoutPanel tableLayoutPanel1;

	private Button okButton;

	private Label lineContentsLabel;

	private Label macroContentsLabel;

	private TextBox causeTextBox;

	private TextBox fileTextBox;

	public MacroExceptionDialog()
	{
		InitializeComponent();
	}

	internal MacroExceptionDialog(MacroException me)
	{
		InitializeComponent();
		lineContentsLabel.Text = me.Line;
		fileTextBox.Text = me.File;
		macroContentsLabel.Text = me.Macro;
		causeTextBox.Text = me.Message;
		if (me.Macros == null)
		{
			macrosListView.Visible = false;
			base.Height = 206;
			return;
		}
		foreach (string key in me.Macros.Keys)
		{
			macrosListView.Items.Add(key);
		}
		macrosListView.Items.Add("Configuration");
		macrosListView.Items.Add("libpath (only valid in Copy section)");
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
		this.pageTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.errorLabel = new System.Windows.Forms.Label();
		this.fileLabel = new System.Windows.Forms.Label();
		this.lineLabel = new System.Windows.Forms.Label();
		this.macroLabel = new System.Windows.Forms.Label();
		this.causeLabel = new System.Windows.Forms.Label();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.macrosListView = new System.Windows.Forms.ListView();
		this.macrosColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.okButton = new System.Windows.Forms.Button();
		this.lineContentsLabel = new System.Windows.Forms.Label();
		this.macroContentsLabel = new System.Windows.Forms.Label();
		this.causeTextBox = new System.Windows.Forms.TextBox();
		this.fileTextBox = new System.Windows.Forms.TextBox();
		this.pageTableLayoutPanel.SuspendLayout();
		this.tableLayoutPanel1.SuspendLayout();
		base.SuspendLayout();
		this.pageTableLayoutPanel.ColumnCount = 2;
		this.pageTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.pageTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.pageTableLayoutPanel.Controls.Add(this.errorLabel, 0, 0);
		this.pageTableLayoutPanel.Controls.Add(this.fileLabel, 0, 1);
		this.pageTableLayoutPanel.Controls.Add(this.lineLabel, 0, 2);
		this.pageTableLayoutPanel.Controls.Add(this.macroLabel, 0, 3);
		this.pageTableLayoutPanel.Controls.Add(this.causeLabel, 0, 4);
		this.pageTableLayoutPanel.Controls.Add(this.tableLayoutPanel1, 0, 6);
		this.pageTableLayoutPanel.Controls.Add(this.lineContentsLabel, 1, 2);
		this.pageTableLayoutPanel.Controls.Add(this.macroContentsLabel, 1, 3);
		this.pageTableLayoutPanel.Controls.Add(this.causeTextBox, 1, 4);
		this.pageTableLayoutPanel.Controls.Add(this.fileTextBox, 1, 1);
		this.pageTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pageTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
		this.pageTableLayoutPanel.Name = "pageTableLayoutPanel";
		this.pageTableLayoutPanel.RowCount = 7;
		this.pageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.pageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.pageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.pageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.pageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.pageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.pageTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.pageTableLayoutPanel.Size = new System.Drawing.Size(426, 392);
		this.pageTableLayoutPanel.TabIndex = 0;
		this.errorLabel.AutoSize = true;
		this.pageTableLayoutPanel.SetColumnSpan(this.errorLabel, 2);
		this.errorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.errorLabel.Location = new System.Drawing.Point(3, 0);
		this.errorLabel.Name = "errorLabel";
		this.errorLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
		this.errorLabel.Size = new System.Drawing.Size(422, 23);
		this.errorLabel.TabIndex = 0;
		this.errorLabel.Text = "Macro parsing error!";
		this.errorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.fileLabel.AutoSize = true;
		this.fileLabel.Dock = System.Windows.Forms.DockStyle.Left;
		this.fileLabel.Location = new System.Drawing.Point(3, 23);
		this.fileLabel.Name = "fileLabel";
		this.fileLabel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 5);
		this.fileLabel.Size = new System.Drawing.Size(26, 21);
		this.fileLabel.TabIndex = 1;
		this.fileLabel.Text = "File:";
		this.fileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.lineLabel.AutoSize = true;
		this.lineLabel.Dock = System.Windows.Forms.DockStyle.Left;
		this.lineLabel.Location = new System.Drawing.Point(3, 44);
		this.lineLabel.Name = "lineLabel";
		this.lineLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
		this.lineLabel.Size = new System.Drawing.Size(30, 23);
		this.lineLabel.TabIndex = 2;
		this.lineLabel.Text = "Line:";
		this.lineLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.macroLabel.AutoSize = true;
		this.macroLabel.Dock = System.Windows.Forms.DockStyle.Left;
		this.macroLabel.Location = new System.Drawing.Point(3, 67);
		this.macroLabel.Name = "macroLabel";
		this.macroLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
		this.macroLabel.Size = new System.Drawing.Size(40, 23);
		this.macroLabel.TabIndex = 3;
		this.macroLabel.Text = "Macro:";
		this.macroLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.causeLabel.AutoSize = true;
		this.causeLabel.Dock = System.Windows.Forms.DockStyle.Left;
		this.causeLabel.Location = new System.Drawing.Point(3, 90);
		this.causeLabel.Name = "causeLabel";
		this.causeLabel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 5);
		this.causeLabel.Size = new System.Drawing.Size(40, 72);
		this.causeLabel.TabIndex = 4;
		this.causeLabel.Text = "Cause:";
		this.causeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.tableLayoutPanel1.ColumnCount = 3;
		this.pageTableLayoutPanel.SetColumnSpan(this.tableLayoutPanel1, 2);
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.Controls.Add(this.macrosListView, 0, 0);
		this.tableLayoutPanel1.Controls.Add(this.okButton, 1, 1);
		this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 165);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 2;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(422, 224);
		this.tableLayoutPanel1.TabIndex = 6;
		this.macrosListView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.macrosListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.macrosListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.macrosColumnHeader });
		this.tableLayoutPanel1.SetColumnSpan(this.macrosListView, 3);
		this.macrosListView.FullRowSelect = true;
		this.macrosListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
		this.macrosListView.Location = new System.Drawing.Point(3, 3);
		this.macrosListView.MultiSelect = false;
		this.macrosListView.Name = "macrosListView";
		this.macrosListView.Size = new System.Drawing.Size(416, 189);
		this.macrosListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
		this.macrosListView.TabIndex = 5;
		this.macrosListView.UseCompatibleStateImageBehavior = false;
		this.macrosListView.View = System.Windows.Forms.View.Details;
		this.macrosColumnHeader.Text = "Valid Macros";
		this.macrosColumnHeader.Width = 415;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.okButton.Location = new System.Drawing.Point(179, 198);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(64, 23);
		this.okButton.TabIndex = 6;
		this.okButton.Text = "OK";
		this.okButton.UseVisualStyleBackColor = true;
		this.lineContentsLabel.AutoSize = true;
		this.lineContentsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lineContentsLabel.Location = new System.Drawing.Point(49, 44);
		this.lineContentsLabel.Name = "lineContentsLabel";
		this.lineContentsLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
		this.lineContentsLabel.Size = new System.Drawing.Size(376, 23);
		this.lineContentsLabel.TabIndex = 8;
		this.lineContentsLabel.Text = "label1";
		this.macroContentsLabel.AutoSize = true;
		this.macroContentsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.macroContentsLabel.Location = new System.Drawing.Point(49, 67);
		this.macroContentsLabel.Name = "macroContentsLabel";
		this.macroContentsLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
		this.macroContentsLabel.Size = new System.Drawing.Size(376, 23);
		this.macroContentsLabel.TabIndex = 9;
		this.macroContentsLabel.Text = "label2";
		this.causeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.causeTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.causeTextBox.Location = new System.Drawing.Point(49, 93);
		this.causeTextBox.Multiline = true;
		this.causeTextBox.Name = "causeTextBox";
		this.causeTextBox.ReadOnly = true;
		this.causeTextBox.Size = new System.Drawing.Size(376, 66);
		this.causeTextBox.TabIndex = 10;
		this.fileTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.fileTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.fileTextBox.Location = new System.Drawing.Point(49, 26);
		this.fileTextBox.Name = "fileTextBox";
		this.fileTextBox.ReadOnly = true;
		this.fileTextBox.Size = new System.Drawing.Size(376, 13);
		this.fileTextBox.TabIndex = 11;
		base.AcceptButton = this.okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.okButton;
		base.ClientSize = new System.Drawing.Size(426, 392);
		base.ControlBox = false;
		base.Controls.Add(this.pageTableLayoutPanel);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(442, 39);
		base.Name = "MacroExceptionDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "MacroExceptionDialog";
		this.pageTableLayoutPanel.ResumeLayout(false);
		this.pageTableLayoutPanel.PerformLayout();
		this.tableLayoutPanel1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
