using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Common.CodeCompletion;

public class GotoPopupWindow : BasePopupControl
{
	private class ItemInfo
	{
		public readonly string Name;

		public readonly string Description;

		public readonly Image ItemImage;

		public readonly object Tag;

		public ItemInfo(string name, string description, Image image, object tag)
		{
			Name = name;
			Description = description;
			ItemImage = image;
			Tag = tag;
		}
	}

	public delegate void AddItemDelegate(string name, string description, int iconIndex, object tag);

	public delegate void FillItemsListDelegate(AddItemDelegate addMethod);

	private const int WM_KEYDOWN = 256;

	private readonly ToolStripMenuItem foundItemsPopupBottom;

	private readonly ToolStripMenuItem foundItemsPopupTop;

	private readonly ItemsListPopupMenu foundItems;

	private SortedList<string, ItemInfo> completeItemsList;

	private FillItemsListDelegate fillItemsListDelegate;

	private ImageList imageList;

	private object clickedItemTag;

	private object tooManyItemsTerminator = new object();

	private IContainer components;

	private TextBox textBox;

	private PictureBox searchIconBox;

	public event EventHandler Closed;

	public event EventHandler<SelectedItemEventArgs> ItemSelected;

	public GotoPopupWindow(FillItemsListDelegate fillMethod, ImageList imageList)
	{
		if (fillMethod == null)
		{
			throw new ArgumentNullException("fillMethod");
		}
		fillItemsListDelegate = fillMethod;
		this.imageList = imageList;
		InitializeComponent();
		searchIconBox.Image = IconService.GetBitmap("Icons.16x16.FindIcon");
		textBox.TextChanged += textBox_TextChanged;
		foundItemsPopupBottom = new ToolStripMenuItem();
		foundItemsPopupBottom.DropDownDirection = ToolStripDropDownDirection.BelowRight;
		foundItemsPopupBottom.AutoSize = false;
		foundItemsPopupBottom.Margin = Padding.Empty;
		foundItemsPopupBottom.Size = new Size(0, 0);
		m_popItUp.Items.Add(foundItemsPopupBottom);
		foundItemsPopupTop = new ToolStripMenuItem();
		foundItemsPopupTop.DropDownDirection = ToolStripDropDownDirection.AboveRight;
		foundItemsPopupTop.AutoSize = false;
		foundItemsPopupTop.Margin = Padding.Empty;
		foundItemsPopupTop.Size = new Size(0, 0);
		m_popItUp.Items.Insert(0, foundItemsPopupTop);
		foundItems = new ItemsListPopupMenu();
		foundItems.Renderer = new GotoPopupMenuRenderer(new GotoPopupMenuColorTable());
		foundItems.ItemClicked += foundItems_ItemClicked;
		foundItemsPopupBottom.DropDown = foundItems;
		foundItemsPopupTop.DropDown = foundItems;
		m_popItUp.Closed += PopupClosed;
		m_popItUp.Closing += PopupClosing;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void AddItem(string name, string description, int iconIndex, object tag)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (completeItemsList != null)
		{
			if (description == null)
			{
				description = string.Empty;
			}
			string key = name + " " + description;
			if (!completeItemsList.ContainsKey(key))
			{
				ItemInfo value = new ItemInfo(name, description, (imageList != null && iconIndex >= 0 && iconIndex < imageList.Images.Count) ? imageList.Images[iconIndex] : null, tag);
				completeItemsList.Add(key, value);
			}
		}
	}

	protected override Point GetPopupLocation(Control control)
	{
		return new Point
		{
			X = control.Width / 2 - base.Width / 2,
			Y = control.Height / 3
		};
	}

	private void textBox_TextChanged(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		if (completeItemsList == null)
		{
			completeItemsList = new SortedList<string, ItemInfo>();
			fillItemsListDelegate(AddItem);
		}
		int i = 0;
		string value;
		for (value = textBox.Text; i < completeItemsList.Keys.Count && !completeItemsList.Keys[i].StartsWith(value, StringComparison.InvariantCultureIgnoreCase); i++)
		{
		}
		foundItemsPopupBottom.HideDropDown();
		foundItemsPopupTop.HideDropDown();
		foundItems.Close(ToolStripDropDownCloseReason.CloseCalled);
		foundItems.Items.Clear();
		if (!string.IsNullOrEmpty(value))
		{
			int num = 0;
			for (; i < completeItemsList.Keys.Count && completeItemsList.Keys[i].StartsWith(value, StringComparison.InvariantCultureIgnoreCase); i++)
			{
				ItemInfo itemInfo = completeItemsList.Values[i];
				ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(itemInfo.Name);
				toolStripMenuItem.ShortcutKeyDisplayString = itemInfo.Description;
				toolStripMenuItem.Image = itemInfo.ItemImage;
				toolStripMenuItem.Tag = itemInfo.Tag;
				foundItems.Items.Add(toolStripMenuItem);
				if (num++ > 50)
				{
					ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem("Too many items in the list");
					toolStripMenuItem2.Tag = tooManyItemsTerminator;
					foundItems.Items.Add(toolStripMenuItem2);
					break;
				}
			}
			if (foundItems.Items.Count > 0)
			{
				ShowFoundItems();
			}
		}
		Cursor.Current = Cursors.Default;
	}

	private void foundItems_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
		clickedItemTag = e.ClickedItem.Tag;
		if (e.ClickedItem.Tag != tooManyItemsTerminator)
		{
			Close();
		}
	}

	private void PopupClosing(object sender, ToolStripDropDownClosingEventArgs e)
	{
		if (e.CloseReason == ToolStripDropDownCloseReason.AppClicked && foundItems.Bounds.Contains(Control.MousePosition))
		{
			e.Cancel = true;
		}
	}

	private void PopupClosed(object sender, ToolStripDropDownClosedEventArgs e)
	{
		foundItems.ItemClicked -= foundItems_ItemClicked;
		foundItemsPopupBottom.HideDropDown();
		foundItemsPopupTop.HideDropDown();
		foundItems.Close(ToolStripDropDownCloseReason.CloseCalled);
		if (clickedItemTag != null)
		{
			OnItemSelected(new SelectedItemEventArgs(clickedItemTag));
		}
		OnClosed();
		m_popItUp.Closed -= PopupClosed;
		foundItemsPopupBottom.DropDown = null;
		foundItemsPopupTop.DropDown = null;
		foundItems.Items.Clear();
		foundItems.Dispose();
		foundItemsPopupBottom.Dispose();
		foundItemsPopupTop.Dispose();
		fillItemsListDelegate = null;
		imageList = null;
		clickedItemTag = null;
		if (completeItemsList != null)
		{
			completeItemsList.Clear();
		}
	}

	private void ShowFoundItems()
	{
		Rectangle workingArea = Screen.FromControl(this).WorkingArea;
		Point point = base.Parent.PointToScreen(base.Location);
		int num = workingArea.Height - point.Y + base.Size.Height;
		int num2 = point.Y;
		Rectangle preferredRect = RectangleToScreen(new Rectangle
		{
			Size = foundItems.GetPreferredSize(Size.Empty)
		});
		if (num2 > num * 2)
		{
			workingArea.Height = PointToScreen(Point.Empty).Y;
			preferredRect.Offset(0, -preferredRect.Height);
			foundItems.Bounds = FitMenuToRect(workingArea, preferredRect);
			foundItemsPopupTop.ShowDropDown();
			foundItemsPopupTop.DropDown.Items[0].Select();
		}
		else
		{
			int num3 = (workingArea.Y = PointToScreen(new Point(0, base.Size.Height)).Y);
			workingArea.Height -= num3;
			preferredRect.Offset(0, base.Size.Height);
			foundItems.Bounds = FitMenuToRect(workingArea, preferredRect);
			foundItemsPopupBottom.ShowDropDown();
			foundItemsPopupBottom.DropDown.Items[0].Select();
		}
		textBox.Focus();
	}

	private static Rectangle FitMenuToRect(Rectangle screen, Rectangle preferredRect)
	{
		if (screen.Contains(preferredRect))
		{
			return preferredRect;
		}
		if (preferredRect.Right > screen.Right)
		{
			preferredRect.X = screen.Right - preferredRect.Right;
		}
		if (preferredRect.Height > screen.Height)
		{
			preferredRect.Y = screen.Y;
			preferredRect.Height = screen.Height;
		}
		return preferredRect;
	}

	private void OnItemSelected(SelectedItemEventArgs args)
	{
		if (this.ItemSelected != null)
		{
			this.ItemSelected(this, args);
		}
	}

	private void OnClosed()
	{
		if (this.Closed != null)
		{
			this.Closed(this, EventArgs.Empty);
		}
	}

	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (m.Msg == 256)
		{
			Keys keys = (Keys)((int)(long)m.WParam & 0xFFFF);
			switch (keys)
			{
			case Keys.Up:
			case Keys.Down:
				foundItems.ProcessArrowKeys(keys);
				return true;
			case Keys.Return:
				foundItems.ProcessEnterKey();
				return true;
			}
		}
		return base.ProcessKeyPreview(ref m);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		Keys keys = keyData & Keys.KeyCode;
		if (keys == Keys.Return)
		{
			foundItems.ProcessEnterKey();
			return true;
		}
		return base.ProcessDialogKey(keyData);
	}

	private void InitializeComponent()
	{
		this.textBox = new System.Windows.Forms.TextBox();
		this.searchIconBox = new System.Windows.Forms.PictureBox();
		System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
		System.Windows.Forms.Label label = new System.Windows.Forms.Label();
		panel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.searchIconBox).BeginInit();
		base.SuspendLayout();
		this.textBox.Location = new System.Drawing.Point(35, 29);
		this.textBox.Name = "textBox";
		this.textBox.Size = new System.Drawing.Size(263, 20);
		this.textBox.TabIndex = 2;
		panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel.Controls.Add(this.textBox);
		panel.Controls.Add(this.searchIconBox);
		panel.Controls.Add(label);
		panel.Dock = System.Windows.Forms.DockStyle.Fill;
		panel.Location = new System.Drawing.Point(0, 0);
		panel.Name = "panel1";
		panel.Size = new System.Drawing.Size(308, 63);
		panel.TabIndex = 1;
		this.searchIconBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.searchIconBox.Location = new System.Drawing.Point(9, 29);
		this.searchIconBox.Name = "searchIconBox";
		this.searchIconBox.Size = new System.Drawing.Size(20, 20);
		this.searchIconBox.TabIndex = 1;
		this.searchIconBox.TabStop = false;
		label.AutoSize = true;
		label.Location = new System.Drawing.Point(6, 8);
		label.Name = "label1";
		label.Size = new System.Drawing.Size(87, 13);
		label.TabIndex = 0;
		label.Text = "Enter type name:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(panel);
		base.Name = "GotoPopupWindow";
		base.Size = new System.Drawing.Size(308, 63);
		panel.ResumeLayout(false);
		panel.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.searchIconBox).EndInit();
		base.ResumeLayout(false);
	}
}
