using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop;
using SoftVelocity.Ide.Core;

namespace SoftVelocity.Generator.UI;

internal class TwoFileForm : Form
{
	private struct WhichDialog
	{
		public TextBox tb;

		public bool forOpen;

		public string filter;

		public WhichDialog(string f, TextBox tb, bool b)
		{
			filter = f;
			this.tb = tb;
			forOpen = b;
		}
	}

	private string initialDirectory;

	private IContainer components;

	internal TextBox txaFile;

	internal TextBox appFile;

	private Button txaEllipsis;

	private Button appEllipsis;

	private Button cancelButton;

	private Button okButton;

	private Label label2;

	private Label label1;

	internal TwoFileForm(string title, string label1, string filter1, bool forOpen1, string label2, string filter2, bool forOpen2)
	{
		base.DialogResult = DialogResult.Cancel;
		InitializeComponent();
		Text = title;
		this.label1.Text = label1;
		this.label2.Text = label2;
		txaEllipsis.Tag = new WhichDialog(filter1, txaFile, forOpen1);
		appEllipsis.Tag = new WhichDialog(filter2, appFile, forOpen2);
		OkButtonEnabler();
		initialDirectory = FileService.CurrentDirectory;
	}

	private void EllipsisPressed(object sender, EventArgs args)
	{
		Button button = (Button)sender;
		WhichDialog whichDialog = (WhichDialog)button.Tag;
		SoftVelocity.Ide.Core.FileDialog fileDialog;
		if (whichDialog.forOpen)
		{
			SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
			openFileDialog.Multiselect = false;
			fileDialog = openFileDialog;
		}
		else
		{
			fileDialog = FileDialogService.SaveFileDialog();
		}
		using (fileDialog)
		{
			fileDialog.Filter = whichDialog.filter;
			fileDialog.CheckFileExists = whichDialog.forOpen;
			fileDialog.AddExtension = true;
			fileDialog.RestoreDirectory = false;
			if (string.IsNullOrEmpty(txaFile.Text) && string.IsNullOrEmpty(appFile.Text))
			{
				fileDialog.InitialDirectory = FileService.CurrentDirectory;
			}
			if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				whichDialog.tb.Text = fileDialog.FileName;
			}
		}
	}

	public string File(int pos)
	{
		return pos switch
		{
			1 => txaFile.Text, 
			2 => appFile.Text, 
			_ => throw new IndexOutOfRangeException("pos must be 1 or 2"), 
		};
	}

	private void OnTextChanged(object sender, EventArgs e)
	{
		OkButtonEnabler();
	}

	private void OkButtonEnabler()
	{
		okButton.Enabled = !string.IsNullOrEmpty(txaFile.Text) && !string.IsNullOrEmpty(appFile.Text);
	}

	private void OnFormClosed(object sender, FormClosedEventArgs e)
	{
		if (base.DialogResult != DialogResult.OK)
		{
			Directory.SetCurrentDirectory(initialDirectory);
			return;
		}
		txaFile.Text = GetFullName(txaFile.Text, null);
		appFile.Text = GetFullName(appFile.Text, txaFile.Text);
	}

	private string GetFullName(string fileName, string baseFileName)
	{
		if (!string.IsNullOrEmpty(fileName))
		{
			string directoryName = Path.GetDirectoryName(fileName);
			if (string.IsNullOrEmpty(directoryName))
			{
				string text = null;
				if (!string.IsNullOrEmpty(baseFileName))
				{
					text = Path.GetDirectoryName(baseFileName);
				}
				if (string.IsNullOrEmpty(text))
				{
					text = Directory.GetCurrentDirectory();
				}
				return Path.Combine(text, fileName);
			}
		}
		return fileName;
	}

	private void OnokButtonClicked(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
	}

	private void appFile_Leave(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(appFile.Text))
		{
			if (string.IsNullOrEmpty(Path.GetExtension(appFile.Text)))
			{
				appFile.Text += ".app";
			}
			appFile.Text = GetFullName(appFile.Text, txaFile.Text);
		}
	}

	private void txaFile_Leave(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(txaFile.Text))
		{
			if (string.IsNullOrEmpty(Path.GetExtension(txaFile.Text)))
			{
				txaFile.Text += ".txa";
			}
			txaFile.Text = GetFullName(txaFile.Text, appFile.Text);
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
		this.txaFile = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.appFile = new System.Windows.Forms.TextBox();
		this.txaEllipsis = new System.Windows.Forms.Button();
		this.appEllipsis = new System.Windows.Forms.Button();
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.txaFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.txaFile.Location = new System.Drawing.Point(132, 14);
		this.txaFile.Margin = new System.Windows.Forms.Padding(4);
		this.txaFile.Name = "txaFile";
		this.txaFile.Size = new System.Drawing.Size(320, 22);
		this.txaFile.TabIndex = 1;
		this.txaFile.TextChanged += new System.EventHandler(OnTextChanged);
		this.txaFile.Leave += new System.EventHandler(txaFile_Leave);
		this.label1.Location = new System.Drawing.Point(16, 14);
		this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(95, 25);
		this.label1.TabIndex = 0;
		this.label1.Text = "txafile";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.UseMnemonic = false;
		this.label2.Location = new System.Drawing.Point(16, 55);
		this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(153, 25);
		this.label2.TabIndex = 0;
		this.label2.Text = "appfile";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.appFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.appFile.Location = new System.Drawing.Point(132, 55);
		this.appFile.Margin = new System.Windows.Forms.Padding(4);
		this.appFile.Name = "appFile";
		this.appFile.Size = new System.Drawing.Size(320, 22);
		this.appFile.TabIndex = 3;
		this.appFile.TextChanged += new System.EventHandler(OnTextChanged);
		this.appFile.Leave += new System.EventHandler(appFile_Leave);
		this.txaEllipsis.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.txaEllipsis.AutoEllipsis = true;
		this.txaEllipsis.Location = new System.Drawing.Point(463, 14);
		this.txaEllipsis.Margin = new System.Windows.Forms.Padding(4);
		this.txaEllipsis.Name = "txaEllipsis";
		this.txaEllipsis.Size = new System.Drawing.Size(37, 25);
		this.txaEllipsis.TabIndex = 2;
		this.txaEllipsis.Text = "...";
		this.txaEllipsis.UseVisualStyleBackColor = true;
		this.txaEllipsis.Click += new System.EventHandler(EllipsisPressed);
		this.appEllipsis.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.appEllipsis.AutoEllipsis = true;
		this.appEllipsis.Location = new System.Drawing.Point(463, 55);
		this.appEllipsis.Margin = new System.Windows.Forms.Padding(4);
		this.appEllipsis.Name = "appEllipsis";
		this.appEllipsis.Size = new System.Drawing.Size(37, 25);
		this.appEllipsis.TabIndex = 4;
		this.appEllipsis.Text = "...";
		this.appEllipsis.UseVisualStyleBackColor = true;
		this.appEllipsis.Click += new System.EventHandler(EllipsisPressed);
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Enabled = false;
		this.okButton.Location = new System.Drawing.Point(11, 90);
		this.okButton.Margin = new System.Windows.Forms.Padding(4);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(100, 28);
		this.okButton.TabIndex = 6;
		this.okButton.Text = "OK";
		this.okButton.UseVisualStyleBackColor = true;
		this.okButton.Click += new System.EventHandler(OnokButtonClicked);
		this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(400, 90);
		this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(100, 28);
		this.cancelButton.TabIndex = 7;
		this.cancelButton.Text = "Cancel";
		this.cancelButton.UseVisualStyleBackColor = true;
		base.AcceptButton = this.okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(509, 124);
		base.ControlBox = false;
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.appEllipsis);
		base.Controls.Add(this.txaEllipsis);
		base.Controls.Add(this.appFile);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.txaFile);
		base.Margin = new System.Windows.Forms.Padding(4);
		this.MaximumSize = new System.Drawing.Size(13333326, 167);
		this.MinimumSize = new System.Drawing.Size(341, 167);
		base.Name = "TwoFileForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Quickstatr";
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(OnFormClosed);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
