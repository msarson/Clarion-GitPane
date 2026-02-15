using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Clarion.Base;
using Clarion.Core.Options;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using SoftVelocity.Ide.Core;

namespace SoftVelocity.Generator.Src.UIBinding.ApplicationEditor.Dialogs;

internal class EditABCPaths : PositionedForm
{
	private ABCReaderPath file;

	private IContainer components;

	private TableLayoutPanel tableLayoutPanel1;

	private Label label1;

	private Label label2;

	private ListBox versionPathsListBox;

	private Button okButton;

	private TableLayoutPanel tableLayoutPanel2;

	private Button cancelButton;

	private Button addButton;

	private Button removeButton;

	private ListBox appPathsListBox;

	private Label label3;

	public EditABCPaths()
	{
		InitializeComponent();
		if (!((Component)this).DesignMode)
		{
			file = ABCReaderPathFiles.GetABCReaderPath(Directory.GetCurrentDirectory());
			InitLists();
		}
	}

	private void InitLists()
	{
		LoadList(versionPathsListBox, Versions.GetVersion(Versions.GetActiveVersion(true), true).Libsrc);
		LoadList(appPathsListBox, file.LibSrc);
	}

	private void LoadList(ListBox listBox, string libSrc)
	{
		ListBox.ObjectCollection items = listBox.Items;
		if (!string.IsNullOrEmpty(libSrc))
		{
			string[] array = libSrc.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string item in array2)
			{
				items.Add(item);
			}
		}
	}

	private void AppPathsSelectedIndexChanged(object sender, EventArgs e)
	{
		if (appPathsListBox.SelectedIndex >= 0)
		{
			removeButton.Enabled = true;
		}
	}

	private void OkClicked(object sender, EventArgs e)
	{
		ListBox.ObjectCollection items = appPathsListBox.Items;
		string text = string.Empty;
		foreach (string item in items)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ";";
			}
			text += item;
		}
		file.LibSrc = text;
		file.Save();
		((Form)this).DialogResult = DialogResult.OK;
		((Form)this).Close();
	}

	private void AddButtonClicked(object sender, EventArgs e)
	{
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.Title = StringParser.Parse("${res:ClarionVersionsOptionsPanel.FindLibsrc.Title}");
		string text = string.Join("|", (string[])AddInTree.GetTreeNode("/ClarionVersionsOptions/ClarionWin/FileFilter/Libsrc").BuildChildItems((object)null).ToArray(typeof(string))) + "|${res:SharpDevelop.FileFilter.AllFiles}|*.*";
		openFileDialog.Filter = StringParser.Parse(text);
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			appPathsListBox.Items.Add(Path.Combine(Path.GetPathRoot(openFileDialog.FileName), Path.GetDirectoryName(openFileDialog.FileName)));
		}
	}

	private void RemoveButtonClicked(object sender, EventArgs e)
	{
		appPathsListBox.Items.RemoveAt(appPathsListBox.SelectedIndex);
	}

	private void DialogLoaded(object sender, EventArgs e)
	{
		if (appPathsListBox.Items.Count > 0)
		{
			appPathsListBox.SelectedIndex = 0;
			appPathsListBox.Select();
		}
		else
		{
			addButton.Select();
		}
	}

	private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((PositionedForm)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		tableLayoutPanel1 = new TableLayoutPanel();
		label1 = new Label();
		label2 = new Label();
		versionPathsListBox = new ListBox();
		tableLayoutPanel2 = new TableLayoutPanel();
		cancelButton = new Button();
		okButton = new Button();
		addButton = new Button();
		removeButton = new Button();
		appPathsListBox = new ListBox();
		label3 = new Label();
		tableLayoutPanel1.SuspendLayout();
		tableLayoutPanel2.SuspendLayout();
		((Control)this).SuspendLayout();
		tableLayoutPanel1.ColumnCount = 5;
		tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12f));
		tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
		tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
		tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12f));
		tableLayoutPanel1.Controls.Add(versionPathsListBox, 2, 3);
		tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 2, 13);
		tableLayoutPanel1.Controls.Add(addButton, 3, 8);
		tableLayoutPanel1.Controls.Add(removeButton, 3, 10);
		tableLayoutPanel1.Controls.Add(appPathsListBox, 2, 7);
		tableLayoutPanel1.Controls.Add(label1, 2, 1);
		tableLayoutPanel1.Controls.Add(label3, 2, 6);
		tableLayoutPanel1.Controls.Add(label2, 2, 2);
		tableLayoutPanel1.Dock = DockStyle.Fill;
		tableLayoutPanel1.Location = new Point(0, 0);
		tableLayoutPanel1.Name = "tableLayoutPanel1";
		tableLayoutPanel1.RowCount = 15;
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle());
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 37f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 8f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 31f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 8f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle());
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle());
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));
		tableLayoutPanel1.RowStyles.Add(new RowStyle());
		tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));
		tableLayoutPanel1.Size = new Size(670, 459);
		tableLayoutPanel1.TabIndex = 0;
		tableLayoutPanel1.Paint += tableLayoutPanel1_Paint;
		label1.AutoSize = true;
		tableLayoutPanel1.SetColumnSpan(label1, 3);
		label1.Dock = DockStyle.Top;
		label1.Location = new Point(15, 12);
		label1.Name = "label1";
		label1.Size = new Size(652, 30);
		label1.TabIndex = 0;
		label1.Text = "This dialog is used to edit the list of paths that the ABC reader\r\nscans for this application (and any other applications in the same folder)";
		label1.TextAlign = ContentAlignment.MiddleCenter;
		label2.AutoSize = true;
		label2.Location = new Point(15, 54);
		label2.Margin = new Padding(3, 12, 3, 0);
		label2.Name = "label2";
		label2.Size = new Size(278, 15);
		label2.TabIndex = 1;
		label2.Text = "Paths scanned by the current version of Clairon are:";
		versionPathsListBox.Dock = DockStyle.Fill;
		versionPathsListBox.ItemHeight = 15;
		versionPathsListBox.Location = new Point(15, 82);
		versionPathsListBox.Name = "versionPathsListBox";
		tableLayoutPanel1.SetRowSpan(versionPathsListBox, 3);
		versionPathsListBox.Size = new Size(547, 108);
		versionPathsListBox.TabIndex = 2;
		tableLayoutPanel2.ColumnCount = 3;
		tableLayoutPanel1.SetColumnSpan(tableLayoutPanel2, 2);
		tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
		tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12f));
		tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
		tableLayoutPanel2.Controls.Add(cancelButton, 2, 0);
		tableLayoutPanel2.Controls.Add(okButton, 0, 0);
		tableLayoutPanel2.Dock = DockStyle.Right;
		tableLayoutPanel2.Location = new Point(443, 411);
		tableLayoutPanel2.Name = "tableLayoutPanel2";
		tableLayoutPanel2.RowCount = 1;
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel2.Size = new Size(212, 33);
		tableLayoutPanel2.TabIndex = 7;
		cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		cancelButton.DialogResult = DialogResult.Cancel;
		cancelButton.Location = new Point(114, 2);
		cancelButton.Margin = new Padding(2);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new Size(96, 25);
		cancelButton.TabIndex = 1;
		cancelButton.Text = "Cancel";
		cancelButton.UseVisualStyleBackColor = true;
		okButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		okButton.Location = new Point(2, 2);
		okButton.Margin = new Padding(2);
		okButton.Name = "okButton";
		okButton.Size = new Size(96, 25);
		okButton.TabIndex = 0;
		okButton.Text = "OK";
		okButton.UseVisualStyleBackColor = true;
		okButton.Click += OkClicked;
		addButton.Location = new Point(568, 235);
		addButton.Name = "addButton";
		addButton.Size = new Size(87, 27);
		addButton.TabIndex = 5;
		addButton.Text = "Add";
		addButton.UseVisualStyleBackColor = true;
		addButton.Click += AddButtonClicked;
		removeButton.Enabled = false;
		removeButton.Location = new Point(568, 354);
		removeButton.Name = "removeButton";
		removeButton.Size = new Size(87, 27);
		removeButton.TabIndex = 6;
		removeButton.Text = "Remove";
		removeButton.UseVisualStyleBackColor = true;
		removeButton.Click += RemoveButtonClicked;
		appPathsListBox.Dock = DockStyle.Fill;
		appPathsListBox.ItemHeight = 15;
		appPathsListBox.Location = new Point(15, 227);
		appPathsListBox.Name = "appPathsListBox";
		tableLayoutPanel1.SetRowSpan(appPathsListBox, 5);
		appPathsListBox.Size = new Size(547, 166);
		appPathsListBox.TabIndex = 4;
		appPathsListBox.SelectedIndexChanged += AppPathsSelectedIndexChanged;
		label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		label3.AutoSize = true;
		label3.Location = new Point(15, 209);
		label3.Name = "label3";
		label3.Size = new Size(190, 15);
		label3.TabIndex = 3;
		label3.Text = "Paths scanned for this Application:";
		((Form)this).AcceptButton = okButton;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(7f, 15f);
		((ContainerControl)this).AutoScaleMode = AutoScaleMode.Font;
		((Form)this).CancelButton = cancelButton;
		((Form)this).ClientSize = new Size(670, 459);
		((Control)this).Controls.Add(tableLayoutPanel1);
		((Control)(object)this).MinimumSize = new Size(686, 391);
		((Control)this).Name = "EditABCPaths";
		((Form)this).ShowIcon = false;
		((Form)this).ShowInTaskbar = false;
		((Control)(object)this).Text = "Edit Paths used by ABC Header Parser";
		((Form)this).Load += DialogLoaded;
		tableLayoutPanel1.ResumeLayout(performLayout: false);
		tableLayoutPanel1.PerformLayout();
		tableLayoutPanel2.ResumeLayout(performLayout: false);
		((Control)this).ResumeLayout(performLayout: false);
	}
}
