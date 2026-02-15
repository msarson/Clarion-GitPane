using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ZetaColorEditor.Runtime.InternalControls;

public class SystemColorEditorUserControl : UserControl
{
	private ListViewItem _needEnsureVisibleListViewItem;

	private static List<Color> colors = new List<Color>();

	private IContainer components;

	private ListView colorsListView;

	private ColumnHeader columnHeader1;

	[Browsable(false)]
	public Color SelectedColor
	{
		get
		{
			if (colorsListView.SelectedItems.Count == 1)
			{
				return (Color)colorsListView.SelectedItems[0].Tag;
			}
			return Color.Empty;
		}
		set
		{
			if (base.DesignMode)
			{
				return;
			}
			foreach (ListViewItem item in colorsListView.Items)
			{
				Color color = (Color)item.Tag;
				if (color == value)
				{
					doSelectItem(item);
					return;
				}
			}
			colorsListView.SelectedItems.Clear();
		}
	}

	public event EventHandler NeedUpdateUI;

	public event EventHandler ColorSelected;

	public SystemColorEditorUserControl()
	{
		InitializeComponent();
	}

	private void DoColorSelected()
	{
		if (this.ColorSelected != null)
		{
			this.ColorSelected(this, EventArgs.Empty);
		}
	}

	private void doSelectItem(ListViewItem listViewItem)
	{
		colorsListView.SelectedItems.Clear();
		listViewItem.Selected = true;
		listViewItem.Focused = true;
		listViewItem.EnsureVisible();
		_needEnsureVisibleListViewItem = listViewItem;
		colorsListView.Select();
		colorsListView.Focus();
	}

	private void colorsListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
	{
		e.DrawDefault = true;
	}

	private void colorsListView_DrawItem(object sender, DrawListViewItemEventArgs e)
	{
		e.DrawDefault = false;
	}

	private void colorsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
	{
		e.DrawDefault = false;
		Color color = (Color)e.Item.Tag;
		Brush highlight;
		Brush brush;
		if (e.Item.Selected)
		{
			highlight = SystemBrushes.Highlight;
			brush = SystemBrushes.HighlightText;
		}
		else
		{
			highlight = SystemBrushes.Window;
			brush = SystemBrushes.WindowText;
		}
		e.Graphics.FillRectangle(highlight, e.Bounds);
		int num = (e.Bounds.Height - 2) * 2;
		int num2 = e.Bounds.Height - 2;
		int num3 = e.Bounds.Left + 1;
		int num4 = e.Bounds.Top + 1;
		Rectangle rect = new Rectangle(num3, num4, num, num2);
		using (Brush brush2 = new SolidBrush(color))
		{
			e.Graphics.FillRectangle(brush2, rect);
		}
		Rectangle rect2 = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
		Pen controlDarkDark = SystemPens.ControlDarkDark;
		e.Graphics.DrawRectangle(controlDarkDark, rect2);
		num3 += num + 2;
		RectangleF layoutRectangle = new RectangleF(num3, e.Bounds.Top, e.Bounds.Width - num3, e.Bounds.Height);
		e.Graphics.DrawString(e.Item.Text, e.Item.Font, brush, layoutRectangle);
	}

	private void systemColorEditorUserControl_Load(object sender, EventArgs e)
	{
		checkEnsureFilled();
		if (_needEnsureVisibleListViewItem != null)
		{
			doSelectItem(_needEnsureVisibleListViewItem);
		}
		colorsListView_SizeChanged(null, null);
	}

	private void colorsListView_SizeChanged(object sender, EventArgs e)
	{
		colorsListView.Columns[0].Width = colorsListView.ClientSize.Width - 1;
	}

	private void colorsListView_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (this.NeedUpdateUI != null)
		{
			this.NeedUpdateUI(this, EventArgs.Empty);
		}
	}

	public bool ContainsColor(Color value)
	{
		checkEnsureFilled();
		foreach (ListViewItem item in colorsListView.Items)
		{
			Color color = (Color)item.Tag;
			if (color == value)
			{
				return true;
			}
		}
		return false;
	}

	private void checkEnsureFilled()
	{
		if (colorsListView.Items.Count > 0)
		{
			return;
		}
		if (colors.Count == 0)
		{
			PropertyInfo[] properties = typeof(SystemColors).GetProperties(BindingFlags.Static | BindingFlags.Public);
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (propertyInfo.PropertyType == typeof(Color))
				{
					Color item = (Color)propertyInfo.GetValue(null, null);
					colors.Add(item);
				}
			}
			int index = 0;
			colors.Sort(delegate(Color c1, Color c2)
			{
				index++;
				return (!(c1 == c2) && !c1.Equals(c2)) ? c1.Name.CompareTo(c2.Name) : 0;
			});
		}
		foreach (Color color in colors)
		{
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.Text = color.Name;
			listViewItem.Tag = color;
			colorsListView.Items.Add(listViewItem);
		}
	}

	private void colorsListView_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			DoColorSelected();
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
		this.colorsListView = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		base.SuspendLayout();
		this.colorsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.columnHeader1 });
		this.colorsListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.colorsListView.FullRowSelect = true;
		this.colorsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
		this.colorsListView.HideSelection = false;
		this.colorsListView.Location = new System.Drawing.Point(0, 0);
		this.colorsListView.Margin = new System.Windows.Forms.Padding(4);
		this.colorsListView.MultiSelect = false;
		this.colorsListView.Name = "colorsListView";
		this.colorsListView.OwnerDraw = true;
		this.colorsListView.ShowGroups = false;
		this.colorsListView.ShowItemToolTips = true;
		this.colorsListView.Size = new System.Drawing.Size(200, 185);
		this.colorsListView.TabIndex = 2;
		this.colorsListView.UseCompatibleStateImageBehavior = false;
		this.colorsListView.View = System.Windows.Forms.View.Details;
		this.colorsListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(colorsListView_MouseDoubleClick);
		this.colorsListView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(colorsListView_DrawColumnHeader);
		this.colorsListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(colorsListView_DrawItem);
		this.colorsListView.SelectedIndexChanged += new System.EventHandler(colorsListView_SelectedIndexChanged);
		this.colorsListView.SizeChanged += new System.EventHandler(colorsListView_SizeChanged);
		this.colorsListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(colorsListView_DrawSubItem);
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.colorsListView);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "SystemColorEditorUserControl";
		base.Size = new System.Drawing.Size(200, 185);
		base.Load += new System.EventHandler(systemColorEditorUserControl_Load);
		base.ResumeLayout(false);
	}
}
