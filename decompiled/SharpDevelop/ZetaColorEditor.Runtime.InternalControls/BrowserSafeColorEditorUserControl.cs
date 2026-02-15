using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ZetaColorEditor.Runtime.InternalControls;

public class BrowserSafeColorEditorUserControl : UserControl
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

	public BrowserSafeColorEditorUserControl()
	{
		InitializeComponent();
	}

	private void colorsListView_Click(object sender, EventArgs e)
	{
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

	private void colorsListView_DoubleClick(object sender, EventArgs e)
	{
		DoColorSelected();
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

	private void browserSafeColorEditorUserControl_Load(object sender, EventArgs e)
	{
		if (!base.DesignMode)
		{
			checkEnsureFilled();
			if (_needEnsureVisibleListViewItem != null)
			{
				doSelectItem(_needEnsureVisibleListViewItem);
			}
			colorsListView_SizeChanged(null, null);
		}
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
			colors.AddRange(new Color[216]
			{
				ColorTranslator.FromHtml("#000000"),
				ColorTranslator.FromHtml("#000033"),
				ColorTranslator.FromHtml("#000066"),
				ColorTranslator.FromHtml("#000099"),
				ColorTranslator.FromHtml("#0000CC"),
				ColorTranslator.FromHtml("#0000FF"),
				ColorTranslator.FromHtml("#003300"),
				ColorTranslator.FromHtml("#003333"),
				ColorTranslator.FromHtml("#003366"),
				ColorTranslator.FromHtml("#003399"),
				ColorTranslator.FromHtml("#0033CC"),
				ColorTranslator.FromHtml("#0033FF"),
				ColorTranslator.FromHtml("#006600"),
				ColorTranslator.FromHtml("#006633"),
				ColorTranslator.FromHtml("#006666"),
				ColorTranslator.FromHtml("#006699"),
				ColorTranslator.FromHtml("#0066CC"),
				ColorTranslator.FromHtml("#0066FF"),
				ColorTranslator.FromHtml("#009900"),
				ColorTranslator.FromHtml("#009933"),
				ColorTranslator.FromHtml("#009966"),
				ColorTranslator.FromHtml("#009999"),
				ColorTranslator.FromHtml("#0099CC"),
				ColorTranslator.FromHtml("#0099FF"),
				ColorTranslator.FromHtml("#00CC00"),
				ColorTranslator.FromHtml("#00CC33"),
				ColorTranslator.FromHtml("#00CC66"),
				ColorTranslator.FromHtml("#00CC99"),
				ColorTranslator.FromHtml("#00CCCC"),
				ColorTranslator.FromHtml("#00CCFF"),
				ColorTranslator.FromHtml("#00FF00"),
				ColorTranslator.FromHtml("#00FF33"),
				ColorTranslator.FromHtml("#00FF66"),
				ColorTranslator.FromHtml("#00FF99"),
				ColorTranslator.FromHtml("#00FFCC"),
				ColorTranslator.FromHtml("#00FFFF"),
				ColorTranslator.FromHtml("#330000"),
				ColorTranslator.FromHtml("#330033"),
				ColorTranslator.FromHtml("#330066"),
				ColorTranslator.FromHtml("#330099"),
				ColorTranslator.FromHtml("#3300CC"),
				ColorTranslator.FromHtml("#3300FF"),
				ColorTranslator.FromHtml("#333300"),
				ColorTranslator.FromHtml("#333333"),
				ColorTranslator.FromHtml("#333366"),
				ColorTranslator.FromHtml("#333399"),
				ColorTranslator.FromHtml("#3333CC"),
				ColorTranslator.FromHtml("#3333FF"),
				ColorTranslator.FromHtml("#336600"),
				ColorTranslator.FromHtml("#336633"),
				ColorTranslator.FromHtml("#336666"),
				ColorTranslator.FromHtml("#336699"),
				ColorTranslator.FromHtml("#3366CC"),
				ColorTranslator.FromHtml("#3366FF"),
				ColorTranslator.FromHtml("#339900"),
				ColorTranslator.FromHtml("#339933"),
				ColorTranslator.FromHtml("#339966"),
				ColorTranslator.FromHtml("#339999"),
				ColorTranslator.FromHtml("#3399CC"),
				ColorTranslator.FromHtml("#3399FF"),
				ColorTranslator.FromHtml("#33CC00"),
				ColorTranslator.FromHtml("#33CC33"),
				ColorTranslator.FromHtml("#33CC66"),
				ColorTranslator.FromHtml("#33CC99"),
				ColorTranslator.FromHtml("#33CCCC"),
				ColorTranslator.FromHtml("#33CCFF"),
				ColorTranslator.FromHtml("#33FF00"),
				ColorTranslator.FromHtml("#33FF33"),
				ColorTranslator.FromHtml("#33FF66"),
				ColorTranslator.FromHtml("#33FF99"),
				ColorTranslator.FromHtml("#33FFCC"),
				ColorTranslator.FromHtml("#33FFFF"),
				ColorTranslator.FromHtml("#660000"),
				ColorTranslator.FromHtml("#660033"),
				ColorTranslator.FromHtml("#660066"),
				ColorTranslator.FromHtml("#660099"),
				ColorTranslator.FromHtml("#6600CC"),
				ColorTranslator.FromHtml("#6600FF"),
				ColorTranslator.FromHtml("#663300"),
				ColorTranslator.FromHtml("#663333"),
				ColorTranslator.FromHtml("#663366"),
				ColorTranslator.FromHtml("#663399"),
				ColorTranslator.FromHtml("#6633CC"),
				ColorTranslator.FromHtml("#6633FF"),
				ColorTranslator.FromHtml("#666600"),
				ColorTranslator.FromHtml("#666633"),
				ColorTranslator.FromHtml("#666666"),
				ColorTranslator.FromHtml("#666699"),
				ColorTranslator.FromHtml("#6666CC"),
				ColorTranslator.FromHtml("#6666FF"),
				ColorTranslator.FromHtml("#669900"),
				ColorTranslator.FromHtml("#669933"),
				ColorTranslator.FromHtml("#669966"),
				ColorTranslator.FromHtml("#669999"),
				ColorTranslator.FromHtml("#6699CC"),
				ColorTranslator.FromHtml("#6699FF"),
				ColorTranslator.FromHtml("#66CC00"),
				ColorTranslator.FromHtml("#66CC33"),
				ColorTranslator.FromHtml("#66CC66"),
				ColorTranslator.FromHtml("#66CC99"),
				ColorTranslator.FromHtml("#66CCCC"),
				ColorTranslator.FromHtml("#66CCFF"),
				ColorTranslator.FromHtml("#66FF00"),
				ColorTranslator.FromHtml("#66FF33"),
				ColorTranslator.FromHtml("#66FF66"),
				ColorTranslator.FromHtml("#66FF99"),
				ColorTranslator.FromHtml("#66FFCC"),
				ColorTranslator.FromHtml("#66FFFF"),
				ColorTranslator.FromHtml("#990000"),
				ColorTranslator.FromHtml("#990033"),
				ColorTranslator.FromHtml("#990066"),
				ColorTranslator.FromHtml("#990099"),
				ColorTranslator.FromHtml("#9900CC"),
				ColorTranslator.FromHtml("#9900FF"),
				ColorTranslator.FromHtml("#993300"),
				ColorTranslator.FromHtml("#993333"),
				ColorTranslator.FromHtml("#993366"),
				ColorTranslator.FromHtml("#993399"),
				ColorTranslator.FromHtml("#9933CC"),
				ColorTranslator.FromHtml("#9933FF"),
				ColorTranslator.FromHtml("#996600"),
				ColorTranslator.FromHtml("#996633"),
				ColorTranslator.FromHtml("#996666"),
				ColorTranslator.FromHtml("#996699"),
				ColorTranslator.FromHtml("#9966CC"),
				ColorTranslator.FromHtml("#9966FF"),
				ColorTranslator.FromHtml("#999900"),
				ColorTranslator.FromHtml("#999933"),
				ColorTranslator.FromHtml("#999966"),
				ColorTranslator.FromHtml("#999999"),
				ColorTranslator.FromHtml("#9999CC"),
				ColorTranslator.FromHtml("#9999FF"),
				ColorTranslator.FromHtml("#99CC00"),
				ColorTranslator.FromHtml("#99CC33"),
				ColorTranslator.FromHtml("#99CC66"),
				ColorTranslator.FromHtml("#99CC99"),
				ColorTranslator.FromHtml("#99CCCC"),
				ColorTranslator.FromHtml("#99CCFF"),
				ColorTranslator.FromHtml("#99FF00"),
				ColorTranslator.FromHtml("#99FF33"),
				ColorTranslator.FromHtml("#99FF66"),
				ColorTranslator.FromHtml("#99FF99"),
				ColorTranslator.FromHtml("#99FFCC"),
				ColorTranslator.FromHtml("#99FFFF"),
				ColorTranslator.FromHtml("#CC0000"),
				ColorTranslator.FromHtml("#CC0033"),
				ColorTranslator.FromHtml("#CC0066"),
				ColorTranslator.FromHtml("#CC0099"),
				ColorTranslator.FromHtml("#CC00CC"),
				ColorTranslator.FromHtml("#CC00FF"),
				ColorTranslator.FromHtml("#CC3300"),
				ColorTranslator.FromHtml("#CC3333"),
				ColorTranslator.FromHtml("#CC3366"),
				ColorTranslator.FromHtml("#CC3399"),
				ColorTranslator.FromHtml("#CC33CC"),
				ColorTranslator.FromHtml("#CC33FF"),
				ColorTranslator.FromHtml("#CC6600"),
				ColorTranslator.FromHtml("#CC6633"),
				ColorTranslator.FromHtml("#CC6666"),
				ColorTranslator.FromHtml("#CC6699"),
				ColorTranslator.FromHtml("#CC66CC"),
				ColorTranslator.FromHtml("#CC66FF"),
				ColorTranslator.FromHtml("#CC9900"),
				ColorTranslator.FromHtml("#CC9933"),
				ColorTranslator.FromHtml("#CC9966"),
				ColorTranslator.FromHtml("#CC9999"),
				ColorTranslator.FromHtml("#CC99CC"),
				ColorTranslator.FromHtml("#CC99FF"),
				ColorTranslator.FromHtml("#CCCC00"),
				ColorTranslator.FromHtml("#CCCC33"),
				ColorTranslator.FromHtml("#CCCC66"),
				ColorTranslator.FromHtml("#CCCC99"),
				ColorTranslator.FromHtml("#CCCCCC"),
				ColorTranslator.FromHtml("#CCCCFF"),
				ColorTranslator.FromHtml("#CCFF00"),
				ColorTranslator.FromHtml("#CCFF33"),
				ColorTranslator.FromHtml("#CCFF66"),
				ColorTranslator.FromHtml("#CCFF99"),
				ColorTranslator.FromHtml("#CCFFCC"),
				ColorTranslator.FromHtml("#CCFFFF"),
				ColorTranslator.FromHtml("#FF0000"),
				ColorTranslator.FromHtml("#FF0033"),
				ColorTranslator.FromHtml("#FF0066"),
				ColorTranslator.FromHtml("#FF0099"),
				ColorTranslator.FromHtml("#FF00CC"),
				ColorTranslator.FromHtml("#FF00FF"),
				ColorTranslator.FromHtml("#FF3300"),
				ColorTranslator.FromHtml("#FF3333"),
				ColorTranslator.FromHtml("#FF3366"),
				ColorTranslator.FromHtml("#FF3399"),
				ColorTranslator.FromHtml("#FF33CC"),
				ColorTranslator.FromHtml("#FF33FF"),
				ColorTranslator.FromHtml("#FF6600"),
				ColorTranslator.FromHtml("#FF6633"),
				ColorTranslator.FromHtml("#FF6666"),
				ColorTranslator.FromHtml("#FF6699"),
				ColorTranslator.FromHtml("#FF66CC"),
				ColorTranslator.FromHtml("#FF66FF"),
				ColorTranslator.FromHtml("#FF9900"),
				ColorTranslator.FromHtml("#FF9933"),
				ColorTranslator.FromHtml("#FF9966"),
				ColorTranslator.FromHtml("#FF9999"),
				ColorTranslator.FromHtml("#FF99CC"),
				ColorTranslator.FromHtml("#FF99FF"),
				ColorTranslator.FromHtml("#FFCC00"),
				ColorTranslator.FromHtml("#FFCC33"),
				ColorTranslator.FromHtml("#FFCC66"),
				ColorTranslator.FromHtml("#FFCC99"),
				ColorTranslator.FromHtml("#FFCCCC"),
				ColorTranslator.FromHtml("#FFCCFF"),
				ColorTranslator.FromHtml("#FFFF00"),
				ColorTranslator.FromHtml("#FFFF33"),
				ColorTranslator.FromHtml("#FFFF66"),
				ColorTranslator.FromHtml("#FFFF99"),
				ColorTranslator.FromHtml("#FFFFCC"),
				ColorTranslator.FromHtml("#FFFFFF")
			});
			int index = 0;
			colors.Sort(delegate(Color c1, Color c2)
			{
				index++;
				if (c1 == c2 || c1.Equals(c2))
				{
					return 0;
				}
				if (c1.A < c2.A)
				{
					return -1;
				}
				if (c1.A > c2.A)
				{
					return 1;
				}
				if (c1.GetHue() < c2.GetHue())
				{
					return -1;
				}
				if (c1.GetHue() > c2.GetHue())
				{
					return 1;
				}
				if (c1.GetSaturation() < c2.GetSaturation())
				{
					return -1;
				}
				if (c1.GetSaturation() > c2.GetSaturation())
				{
					return 1;
				}
				if (c1.GetBrightness() < c2.GetBrightness())
				{
					return -1;
				}
				return (c1.GetBrightness() > c2.GetBrightness()) ? 1 : 0;
			});
		}
		foreach (Color color in colors)
		{
			string text = ColorTranslator.ToHtml(color);
			if (color.IsNamedColor)
			{
				text = text + ", " + color.Name;
			}
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.Text = text;
			listViewItem.Tag = color;
			colorsListView.Items.Add(listViewItem);
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
		this.colorsListView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.colorsListView.MultiSelect = false;
		this.colorsListView.Name = "colorsListView";
		this.colorsListView.OwnerDraw = true;
		this.colorsListView.ShowGroups = false;
		this.colorsListView.ShowItemToolTips = true;
		this.colorsListView.Size = new System.Drawing.Size(200, 185);
		this.colorsListView.TabIndex = 1;
		this.colorsListView.UseCompatibleStateImageBehavior = false;
		this.colorsListView.View = System.Windows.Forms.View.Details;
		this.colorsListView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(colorsListView_DrawColumnHeader);
		this.colorsListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(colorsListView_DrawItem);
		this.colorsListView.SelectedIndexChanged += new System.EventHandler(colorsListView_SelectedIndexChanged);
		this.colorsListView.SizeChanged += new System.EventHandler(colorsListView_SizeChanged);
		this.colorsListView.DoubleClick += new System.EventHandler(colorsListView_DoubleClick);
		this.colorsListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(colorsListView_DrawSubItem);
		this.colorsListView.Click += new System.EventHandler(colorsListView_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.colorsListView);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.Name = "BrowserSafeColorEditorUserControl";
		base.Size = new System.Drawing.Size(200, 185);
		base.Load += new System.EventHandler(browserSafeColorEditorUserControl_Load);
		base.ResumeLayout(false);
	}
}
