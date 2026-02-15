using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class CodeGenerationForm : Form
{
	private ListView categoryListView;

	private Label statusLabel;

	private CheckedListBox selectionListBox;

	private TextEditorControl textEditorControl;

	private Button cancelButton;

	private Button okButton;

	private CodeGeneratorBase SelectedCodeGenerator
	{
		get
		{
			if (categoryListView.SelectedItems.Count <= 0)
			{
				return null;
			}
			return (CodeGeneratorBase)categoryListView.SelectedItems[0].Tag;
		}
	}

	public CodeGenerationForm(TextEditorControl textEditorControl, CodeGeneratorBase[] codeGenerators, IClass currentClass)
	{
		this.textEditorControl = textEditorControl;
		foreach (CodeGeneratorBase codeGeneratorBase in codeGenerators)
		{
			codeGeneratorBase.Initialize(currentClass);
		}
		InitializeComponents();
		okButton.Text = ResourceService.GetString("Global.OKButtonText");
		cancelButton.Text = ResourceService.GetString("Global.CancelButtonText");
		TextLocation position = textEditorControl.ActiveTextAreaControl.Caret.Position;
		TextArea textArea = textEditorControl.ActiveTextAreaControl.TextArea;
		TextView textView = textArea.TextView;
		int visibleLine = textView.Document.GetVisibleLine(position.Y);
		Point p = new Point(textView.GetDrawingXPos(position.Y, position.X) + textView.DrawingPosition.X, (1 + visibleLine) * textView.FontHeight - textArea.VirtualTop.Y - 1 + textView.DrawingPosition.Y);
		Point point = textEditorControl.ActiveTextAreaControl.TextArea.PointToScreen(p);
		point.Y = ((point.Y + base.Height > Screen.FromPoint(point).WorkingArea.Bottom) ? (Screen.FromPoint(point).WorkingArea.Bottom - base.Height) : point.Y);
		point.X = ((point.X + base.Width > Screen.FromPoint(point).WorkingArea.Right) ? (Screen.FromPoint(point).WorkingArea.Right - base.Width) : point.X);
		base.Location = point;
		base.StartPosition = FormStartPosition.Manual;
		categoryListView.SmallImageList = (categoryListView.LargeImageList = ClassBrowserIconService.ImageList);
		foreach (CodeGeneratorBase codeGeneratorBase2 in codeGenerators)
		{
			if (codeGeneratorBase2.IsActive)
			{
				ListViewItem value = new ListViewItem(StringParser.Parse(codeGeneratorBase2.CategoryName))
				{
					ImageIndex = codeGeneratorBase2.ImageIndex,
					Tag = codeGeneratorBase2
				};
				categoryListView.Items.Add(value);
			}
		}
		categoryListView.SelectedIndexChanged += CategoryListViewItemChanged;
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		if (categoryListView.Items.Count > 0)
		{
			categoryListView.Select();
			categoryListView.Focus();
			ListViewItem listViewItem = categoryListView.Items[0];
			bool focused = (categoryListView.Items[0].Selected = true);
			listViewItem.Focused = focused;
		}
		else
		{
			Close();
		}
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		switch (keyData)
		{
		case Keys.Escape:
			Close();
			return true;
		case Keys.Back:
			categoryListView.Focus();
			return true;
		case Keys.Return:
			if (SelectedCodeGenerator != null)
			{
				if (categoryListView.Focused && SelectedCodeGenerator.Content.Count > 0)
				{
					selectionListBox.Focus();
				}
				else
				{
					Close();
					CodeGeneratorBase selectedCodeGenerator = SelectedCodeGenerator;
					TextArea textArea = textEditorControl.ActiveTextAreaControl.TextArea;
					object items;
					if (selectionListBox.CheckedItems.Count <= 0)
					{
						IList selectedItems = selectionListBox.SelectedItems;
						items = selectedItems;
					}
					else
					{
						items = selectionListBox.CheckedItems;
					}
					selectedCodeGenerator.GenerateCode(textArea, (IList)items);
				}
				return true;
			}
			return false;
		default:
			return base.ProcessDialogKey(keyData);
		}
	}

	private void CategoryListViewItemChanged(object sender, EventArgs e)
	{
		CodeGeneratorBase selectedCodeGenerator = SelectedCodeGenerator;
		if (selectedCodeGenerator == null)
		{
			return;
		}
		statusLabel.Text = StringParser.Parse(selectedCodeGenerator.Hint);
		selectionListBox.BeginUpdate();
		selectionListBox.Items.Clear();
		if (selectedCodeGenerator.Content.Count > 0)
		{
			Hashtable hashtable = new Hashtable();
			foreach (object item in selectedCodeGenerator.Content)
			{
				if (!hashtable.Contains(item.ToString()))
				{
					selectionListBox.Items.Add(item);
					hashtable.Add(item.ToString(), "");
				}
			}
			selectionListBox.SelectedIndex = 0;
		}
		selectionListBox.EndUpdate();
		selectionListBox.Refresh();
	}

	private void InitializeComponents()
	{
		okButton = new Button();
		cancelButton = new Button();
		selectionListBox = new CheckedListBox();
		statusLabel = new Label();
		categoryListView = new ListView();
		ColumnHeader columnHeader = new ColumnHeader();
		Panel panel = new Panel();
		panel.SuspendLayout();
		SuspendLayout();
		columnHeader.Width = 258;
		panel.BackColor = SystemColors.Control;
		panel.Controls.Add(okButton);
		panel.Controls.Add(cancelButton);
		panel.Dock = DockStyle.Bottom;
		panel.Location = new Point(1, 309);
		panel.Name = "panel1";
		panel.Size = new Size(262, 29);
		panel.TabIndex = 3;
		okButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		okButton.Location = new Point(94, 3);
		okButton.Name = "okButton";
		okButton.Size = new Size(75, 23);
		okButton.TabIndex = 0;
		okButton.Text = "OK";
		okButton.UseCompatibleTextRendering = true;
		okButton.UseVisualStyleBackColor = true;
		okButton.Click += OkButtonClick;
		cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		cancelButton.DialogResult = DialogResult.Cancel;
		cancelButton.Location = new Point(175, 3);
		cancelButton.Name = "cancelButton";
		cancelButton.Size = new Size(75, 23);
		cancelButton.TabIndex = 1;
		cancelButton.Text = "Cancel";
		cancelButton.UseCompatibleTextRendering = true;
		cancelButton.UseVisualStyleBackColor = true;
		cancelButton.Click += CancelButtonClick;
		selectionListBox.Dock = DockStyle.Fill;
		selectionListBox.IntegralHeight = false;
		selectionListBox.Location = new Point(1, 129);
		selectionListBox.Name = "selectionListBox";
		selectionListBox.Size = new Size(262, 180);
		selectionListBox.TabIndex = 2;
		selectionListBox.UseCompatibleTextRendering = true;
		statusLabel.BackColor = SystemColors.Control;
		statusLabel.Dock = DockStyle.Top;
		statusLabel.Location = new Point(1, 113);
		statusLabel.Name = "statusLabel";
		statusLabel.Size = new Size(262, 16);
		statusLabel.TabIndex = 1;
		statusLabel.Text = "statusLabel";
		statusLabel.UseCompatibleTextRendering = true;
		categoryListView.Columns.AddRange(new ColumnHeader[1] { columnHeader });
		categoryListView.Dock = DockStyle.Top;
		categoryListView.HeaderStyle = ColumnHeaderStyle.None;
		categoryListView.Location = new Point(1, 1);
		categoryListView.MultiSelect = false;
		categoryListView.Name = "categoryListView";
		categoryListView.Size = new Size(262, 112);
		categoryListView.TabIndex = 0;
		categoryListView.UseCompatibleStateImageBehavior = false;
		categoryListView.View = View.Details;
		base.AcceptButton = okButton;
		BackColor = SystemColors.ControlDarkDark;
		base.CancelButton = cancelButton;
		base.ClientSize = new Size(264, 339);
		base.Controls.Add(selectionListBox);
		base.Controls.Add(statusLabel);
		base.Controls.Add(categoryListView);
		base.Controls.Add(panel);
		base.FormBorderStyle = FormBorderStyle.None;
		base.Name = "CodeGenerationForm";
		base.Padding = new Padding(1);
		base.ShowInTaskbar = false;
		panel.ResumeLayout(performLayout: false);
		ResumeLayout(performLayout: false);
	}

	private void CancelButtonClick(object sender, EventArgs e)
	{
		ProcessDialogKey(Keys.Escape);
	}

	private void OkButtonClick(object sender, EventArgs e)
	{
		ProcessDialogKey(Keys.Return);
	}
}
