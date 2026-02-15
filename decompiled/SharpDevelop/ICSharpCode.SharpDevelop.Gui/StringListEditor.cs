using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class StringListEditor : UserControl
{
	private Button removeButton;

	private Button deleteButton;

	private Button moveDownButton;

	private Button moveUpButton;

	private ListBox listBox;

	private Label listLabel;

	private Button updateButton;

	private Button addButton;

	private TextBox editTextBox;

	private Button browseButton;

	private Label TitleLabel;

	private bool browseForDirectory;

	private bool autoAddAfterBrowse;

	public bool ManualOrder
	{
		get
		{
			return !listBox.Sorted;
		}
		set
		{
			Button button = moveUpButton;
			Button button2 = moveDownButton;
			bool flag = (deleteButton.Visible = value);
			bool visible = (button2.Visible = flag);
			button.Visible = visible;
			removeButton.Visible = !value;
			listBox.Sorted = !value;
		}
	}

	public bool BrowseForDirectory
	{
		get
		{
			return browseForDirectory;
		}
		set
		{
			browseForDirectory = value;
			browseButton.Visible = browseForDirectory;
		}
	}

	public bool AutoAddAfterBrowse
	{
		get
		{
			return autoAddAfterBrowse;
		}
		set
		{
			autoAddAfterBrowse = value;
		}
	}

	public string TitleText
	{
		get
		{
			return TitleLabel.Text;
		}
		set
		{
			TitleLabel.Text = value;
		}
	}

	public string AddButtonText
	{
		get
		{
			return addButton.Text;
		}
		set
		{
			addButton.Text = value;
		}
	}

	public string ListCaption
	{
		get
		{
			return listLabel.Text;
		}
		set
		{
			listLabel.Text = value;
		}
	}

	public event EventHandler ListChanged;

	public StringListEditor()
	{
		InitializeComponent();
		Dock = DockStyle.Fill;
		ManualOrder = true;
		BrowseForDirectory = false;
		ListBoxSelectedIndexChanged(null, null);
		EditTextBoxTextChanged(null, null);
		updateButton.Text = StringParser.Parse(updateButton.Text);
		removeButton.Text = StringParser.Parse(removeButton.Text);
		moveUpButton.Image = ResourceService.GetBitmap("Icons.16x16.ArrowUp");
		moveDownButton.Image = ResourceService.GetBitmap("Icons.16x16.ArrowDown");
		deleteButton.Image = ResourceService.GetBitmap("Icons.16x16.DeleteIcon");
	}

	private void InitializeComponent()
	{
		this.removeButton = new System.Windows.Forms.Button();
		this.deleteButton = new System.Windows.Forms.Button();
		this.moveDownButton = new System.Windows.Forms.Button();
		this.moveUpButton = new System.Windows.Forms.Button();
		this.listBox = new System.Windows.Forms.ListBox();
		this.listLabel = new System.Windows.Forms.Label();
		this.updateButton = new System.Windows.Forms.Button();
		this.addButton = new System.Windows.Forms.Button();
		this.editTextBox = new System.Windows.Forms.TextBox();
		this.browseButton = new System.Windows.Forms.Button();
		this.TitleLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.removeButton.Location = new System.Drawing.Point(209, 113);
		this.removeButton.Name = "removeButton";
		this.removeButton.Size = new System.Drawing.Size(75, 23);
		this.removeButton.TabIndex = 5;
		this.removeButton.Text = "${res:Global.DeleteButtonText}";
		this.removeButton.Click += new System.EventHandler(RemoveButtonClick);
		this.deleteButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.deleteButton.Location = new System.Drawing.Point(549, 252);
		this.deleteButton.Name = "deleteButton";
		this.deleteButton.Size = new System.Drawing.Size(24, 24);
		this.deleteButton.TabIndex = 10;
		this.deleteButton.Click += new System.EventHandler(RemoveButtonClick);
		this.moveDownButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.moveDownButton.Location = new System.Drawing.Point(549, 222);
		this.moveDownButton.Name = "moveDownButton";
		this.moveDownButton.Size = new System.Drawing.Size(24, 24);
		this.moveDownButton.TabIndex = 9;
		this.moveDownButton.Click += new System.EventHandler(MoveDownButtonClick);
		this.moveUpButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.moveUpButton.Location = new System.Drawing.Point(549, 192);
		this.moveUpButton.Name = "moveUpButton";
		this.moveUpButton.Size = new System.Drawing.Size(24, 24);
		this.moveUpButton.TabIndex = 8;
		this.moveUpButton.Click += new System.EventHandler(MoveUpButtonClick);
		this.listBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.listBox.FormattingEnabled = true;
		this.listBox.ItemHeight = 16;
		this.listBox.Location = new System.Drawing.Point(47, 177);
		this.listBox.Name = "listBox";
		this.listBox.Size = new System.Drawing.Size(487, 212);
		this.listBox.TabIndex = 7;
		this.listBox.SelectedIndexChanged += new System.EventHandler(ListBoxSelectedIndexChanged);
		this.listLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.listLabel.Location = new System.Drawing.Point(46, 154);
		this.listLabel.Name = "listLabel";
		this.listLabel.Size = new System.Drawing.Size(488, 23);
		this.listLabel.TabIndex = 6;
		this.listLabel.Text = "List:";
		this.updateButton.Location = new System.Drawing.Point(128, 113);
		this.updateButton.Name = "updateButton";
		this.updateButton.Size = new System.Drawing.Size(75, 23);
		this.updateButton.TabIndex = 4;
		this.updateButton.Text = "${res:Global.UpdateButtonText}";
		this.updateButton.Click += new System.EventHandler(UpdateButtonClick);
		this.addButton.Location = new System.Drawing.Point(47, 113);
		this.addButton.Name = "addButton";
		this.addButton.Size = new System.Drawing.Size(75, 23);
		this.addButton.TabIndex = 3;
		this.addButton.Text = "Add Item";
		this.addButton.Click += new System.EventHandler(AddButtonClick);
		this.editTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.editTextBox.Location = new System.Drawing.Point(46, 81);
		this.editTextBox.Name = "editTextBox";
		this.editTextBox.Size = new System.Drawing.Size(488, 22);
		this.editTextBox.TabIndex = 1;
		this.editTextBox.TextChanged += new System.EventHandler(EditTextBoxTextChanged);
		this.browseButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.browseButton.Location = new System.Drawing.Point(545, 81);
		this.browseButton.Name = "browseButton";
		this.browseButton.Size = new System.Drawing.Size(28, 23);
		this.browseButton.TabIndex = 2;
		this.browseButton.Text = "...";
		this.browseButton.Click += new System.EventHandler(BrowseButtonClick);
		this.TitleLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.TitleLabel.Location = new System.Drawing.Point(46, 52);
		this.TitleLabel.Name = "TitleLabel";
		this.TitleLabel.Size = new System.Drawing.Size(488, 22);
		this.TitleLabel.TabIndex = 0;
		this.TitleLabel.Text = "Title:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.removeButton);
		base.Controls.Add(this.deleteButton);
		base.Controls.Add(this.moveDownButton);
		base.Controls.Add(this.moveUpButton);
		base.Controls.Add(this.listBox);
		base.Controls.Add(this.listLabel);
		base.Controls.Add(this.updateButton);
		base.Controls.Add(this.addButton);
		base.Controls.Add(this.editTextBox);
		base.Controls.Add(this.browseButton);
		base.Controls.Add(this.TitleLabel);
		base.Name = "StringListEditor";
		base.Size = new System.Drawing.Size(635, 503);
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	protected virtual void OnListChanged(EventArgs e)
	{
		if (this.ListChanged != null)
		{
			this.ListChanged(this, e);
		}
	}

	public void LoadList(IEnumerable<string> list)
	{
		listBox.Items.Clear();
		foreach (string item in list)
		{
			listBox.Items.Add(item);
		}
	}

	public string[] GetList()
	{
		string[] array = new string[listBox.Items.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = listBox.Items[i].ToString();
		}
		return array;
	}

	private void BrowseButtonClick(object sender, EventArgs e)
	{
		using FolderBrowserDialog folderBrowserDialog = FileService.CreateFolderBrowserDialog("${res:Dialog.ProjectOptions.SelectFolderTitle}");
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			string text = folderBrowserDialog.SelectedPath;
			if (!text.EndsWith("\\") && !text.EndsWith("/"))
			{
				text += "\\";
			}
			editTextBox.Text = text;
			if (autoAddAfterBrowse)
			{
				AddButtonClick(null, null);
			}
		}
	}

	private void AddButtonClick(object sender, EventArgs e)
	{
		editTextBox.Text = editTextBox.Text.Trim();
		if (editTextBox.TextLength > 0)
		{
			int num = listBox.Items.IndexOf(editTextBox.Text);
			if (num < 0)
			{
				num = listBox.Items.Add(editTextBox.Text);
				OnListChanged(EventArgs.Empty);
			}
			listBox.SelectedIndex = num;
		}
	}

	private void UpdateButtonClick(object sender, EventArgs e)
	{
		editTextBox.Text = editTextBox.Text.Trim();
		if (editTextBox.TextLength > 0)
		{
			listBox.Items[listBox.SelectedIndex] = editTextBox.Text;
			OnListChanged(EventArgs.Empty);
		}
	}

	private void RemoveButtonClick(object sender, EventArgs e)
	{
		listBox.Items.RemoveAt(listBox.SelectedIndex);
		OnListChanged(EventArgs.Empty);
	}

	private void MoveUpButtonClick(object sender, EventArgs e)
	{
		int selectedIndex = listBox.SelectedIndex;
		object value = listBox.Items[selectedIndex];
		listBox.Items[selectedIndex] = listBox.Items[selectedIndex - 1];
		listBox.Items[selectedIndex - 1] = value;
		listBox.SelectedIndex = selectedIndex - 1;
		OnListChanged(EventArgs.Empty);
	}

	private void MoveDownButtonClick(object sender, EventArgs e)
	{
		int selectedIndex = listBox.SelectedIndex;
		object value = listBox.Items[selectedIndex];
		listBox.Items[selectedIndex] = listBox.Items[selectedIndex + 1];
		listBox.Items[selectedIndex + 1] = value;
		listBox.SelectedIndex = selectedIndex + 1;
		OnListChanged(EventArgs.Empty);
	}

	private void ListBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		if (listBox.SelectedIndex >= 0)
		{
			editTextBox.Text = listBox.Items[listBox.SelectedIndex].ToString();
		}
		moveUpButton.Enabled = listBox.SelectedIndex > 0;
		moveDownButton.Enabled = listBox.SelectedIndex >= 0 && listBox.SelectedIndex < listBox.Items.Count - 1;
		Button button = removeButton;
		bool enabled = (deleteButton.Enabled = listBox.SelectedIndex >= 0);
		button.Enabled = enabled;
		updateButton.Enabled = listBox.SelectedIndex >= 0 && editTextBox.TextLength > 0;
	}

	private void EditTextBoxTextChanged(object sender, EventArgs e)
	{
		addButton.Enabled = editTextBox.TextLength > 0;
		updateButton.Enabled = listBox.SelectedIndex >= 0 && editTextBox.TextLength > 0;
	}
}
