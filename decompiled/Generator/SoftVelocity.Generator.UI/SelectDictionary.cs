using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SoftVelocity.Ide.Core;

namespace SoftVelocity.Generator.UI;

public class SelectDictionary : Form
{
	private SoftVelocity.Ide.Core.OpenFileDialog openFileDialog1;

	private string _FileName = string.Empty;

	private IContainer components;

	private Button okButton;

	private Button cancelButton;

	private Label label1;

	private TextBox textBox1;

	private Button fileLookupButton;

	private CheckBox checkBoxCopyLocally;

	private string FileName
	{
		get
		{
			if (base.DialogResult == DialogResult.OK && File.Exists(textBox1.Text))
			{
				return textBox1.Text;
			}
			if (File.Exists(_FileName))
			{
				return _FileName;
			}
			return string.Empty;
		}
	}

	public SelectDictionary()
	{
		InitializeComponent();
		openFileDialog1 = FileDialogService.OpenFileDialog();
		openFileDialog1.AddExtension = true;
		openFileDialog1.CheckFileExists = true;
		openFileDialog1.CheckPathExists = true;
		openFileDialog1.DefaultExt = "dct";
		openFileDialog1.FileName = "";
		openFileDialog1.Filter = "DCT|*.dct|All Files|*.*";
		openFileDialog1.FilterIndex = 1;
		openFileDialog1.ForceInitialDirectory = true;
		openFileDialog1.InitialDirectory = "";
		openFileDialog1.Multiselect = false;
		openFileDialog1.RestoreDirectory = false;
		openFileDialog1.Title = "Select a Dictionary";
	}

	public static string Show(string fileName)
	{
		return Show(null, fileName);
	}

	public static string Show(string path, string fileName)
	{
		return Show(null, path, fileName);
	}

	public static string Show(IWin32Window parent, string path, string fileName)
	{
		return Show(parent, path, fileName, allowcopy: false);
	}

	public static string Show(IWin32Window parent, string path, string fileName, bool allowcopy)
	{
		using SelectDictionary selectDictionary = new SelectDictionary();
		if (!string.IsNullOrEmpty(path))
		{
			selectDictionary.openFileDialog1.InitialDirectory = path;
		}
		selectDictionary._FileName = fileName;
		selectDictionary.textBox1.Text = fileName;
		if (allowcopy)
		{
			selectDictionary.checkBoxCopyLocally.Visible = true;
			selectDictionary.checkBoxCopyLocally.Checked = true;
		}
		else
		{
			selectDictionary.checkBoxCopyLocally.Visible = false;
		}
		if (selectDictionary.ShowDialog(parent) != DialogResult.OK)
		{
			return null;
		}
		if (allowcopy && selectDictionary.checkBoxCopyLocally.Checked && !Path.GetFullPath(selectDictionary.FileName).Equals(Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase) && Directory.Exists(Path.GetFullPath(path)))
		{
			string text = Path.Combine(path, Path.GetFileName(selectDictionary.FileName));
			bool flag = true;
			if (File.Exists(text))
			{
				flag = false;
				if (MessageBox.Show("The file already exist.\r\nDo you want to overwriting the file?\r\nFile Name:\r\n" + text, "Confirm Overwriting", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					flag = true;
				}
			}
			if (flag)
			{
				File.Copy(selectDictionary.FileName, text, overwrite: true);
			}
			selectDictionary._FileName = text;
		}
		return selectDictionary.FileName;
	}

	private void okButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void cancelButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void fileLookupButton_Click(object sender, EventArgs e)
	{
		if (openFileDialog1.ShowDialog() == DialogResult.OK)
		{
			textBox1.Text = openFileDialog1.FileName;
			okButton.Enabled = File.Exists(textBox1.Text);
		}
	}

	private void textBox1_Validated(object sender, EventArgs e)
	{
	}

	private void textBox1_Validating(object sender, CancelEventArgs e)
	{
		if (!string.IsNullOrEmpty(textBox1.Text))
		{
			e.Cancel = !File.Exists(textBox1.Text);
		}
		else
		{
			e.Cancel = true;
		}
		okButton.Enabled = !e.Cancel;
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
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
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.fileLookupButton = new System.Windows.Forms.Button();
		this.checkBoxCopyLocally = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.okButton.Location = new System.Drawing.Point(257, 59);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(90, 32);
		this.okButton.TabIndex = 0;
		this.okButton.Text = "&Ok";
		this.okButton.UseVisualStyleBackColor = true;
		this.okButton.Click += new System.EventHandler(okButton_Click);
		this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.cancelButton.Location = new System.Drawing.Point(353, 59);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(86, 32);
		this.cancelButton.TabIndex = 1;
		this.cancelButton.Text = "&Cancel";
		this.cancelButton.UseVisualStyleBackColor = true;
		this.cancelButton.Click += new System.EventHandler(cancelButton_Click);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(27, 24);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(57, 13);
		this.label1.TabIndex = 2;
		this.label1.Text = "Dictionary:";
		this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.textBox1.Location = new System.Drawing.Point(110, 21);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(293, 20);
		this.textBox1.TabIndex = 3;
		this.textBox1.TextChanged += new System.EventHandler(textBox1_TextChanged);
		this.fileLookupButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.fileLookupButton.Location = new System.Drawing.Point(409, 21);
		this.fileLookupButton.Name = "fileLookupButton";
		this.fileLookupButton.Size = new System.Drawing.Size(30, 23);
		this.fileLookupButton.TabIndex = 4;
		this.fileLookupButton.Text = "...";
		this.fileLookupButton.UseVisualStyleBackColor = true;
		this.fileLookupButton.Click += new System.EventHandler(fileLookupButton_Click);
		this.checkBoxCopyLocally.AutoSize = true;
		this.checkBoxCopyLocally.Location = new System.Drawing.Point(30, 66);
		this.checkBoxCopyLocally.Name = "checkBoxCopyLocally";
		this.checkBoxCopyLocally.Size = new System.Drawing.Size(86, 17);
		this.checkBoxCopyLocally.TabIndex = 5;
		this.checkBoxCopyLocally.Text = "Copy Locally";
		this.checkBoxCopyLocally.UseVisualStyleBackColor = true;
		base.ClientSize = new System.Drawing.Size(450, 103);
		base.Controls.Add(this.checkBoxCopyLocally);
		base.Controls.Add(this.fileLookupButton);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.textBox1);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SelectDictionary";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Select a Dictionary";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
