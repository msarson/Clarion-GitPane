using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ZetaColorEditor.Runtime.Colors;

namespace ZetaColorEditor.Runtime.InternalControls;

public class SchemesColorEditorUserControl : UserControl
{
	private bool _ignoreListViewChanges;

	private bool _ignoreComboBoxChanges = true;

	private IColorScheme[] _cacheFor_ColorSchemes;

	private ListViewItem _needEnsureVisibleListViewItem;

	private static Dictionary<string, Color> _knownColorNames;

	private static Dictionary<Color, string> _knownColorValues;

	private IContainer components;

	private ListView colorsListView;

	private ColumnHeader columnHeader1;

	private ComboBox schemesComboBox;

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
			checkEnsureFilled();
			foreach (ListViewItem item in colorsListView.Items)
			{
				Color color = (Color)item.Tag;
				if (color == value)
				{
					colorsListView.SelectedItems.Clear();
					doSelectItem(item);
					return;
				}
			}
			int selectedIndex = schemesComboBox.SelectedIndex;
			for (int i = 0; i < schemesComboBox.Items.Count; i++)
			{
				if (i == selectedIndex)
				{
					continue;
				}
				IColorScheme colorScheme = (IColorScheme)schemesComboBox.Items[i];
				Color[] colors = colorScheme.Colors;
				foreach (Color color2 in colors)
				{
					if (!(color2 == value))
					{
						continue;
					}
					schemesComboBox.SelectedItem = colorScheme;
					foreach (ListViewItem item2 in colorsListView.Items)
					{
						Color color3 = (Color)item2.Tag;
						if (color3 == value)
						{
							doSelectItem(item2);
							return;
						}
					}
				}
			}
			colorsListView.SelectedItems.Clear();
		}
	}

	private IExternalColorEditorInformationProvider externalColorEditorInformationProvider
	{
		get
		{
			Control control = base.Parent;
			while (control != null && !(control is ColorEditorUserControl))
			{
				control = control.Parent;
			}
			if (control == null)
			{
				return null;
			}
			ColorEditorUserControl colorEditorUserControl = (ColorEditorUserControl)control;
			return colorEditorUserControl.ExternalColorEditorInformationProvider;
		}
	}

	private IColorScheme[] colorSchemes
	{
		get
		{
			if (_cacheFor_ColorSchemes == null)
			{
				IExternalColorEditorInformationProvider externalColorEditorInformationProvider = this.externalColorEditorInformationProvider;
				if (externalColorEditorInformationProvider == null || externalColorEditorInformationProvider.ColorSchemes == null)
				{
					_cacheFor_ColorSchemes = null;
				}
				else
				{
					_cacheFor_ColorSchemes = externalColorEditorInformationProvider.ColorSchemes;
				}
			}
			return _cacheFor_ColorSchemes;
		}
	}

	private string storeID => $"{((ColorEditorUserControl)base.Parent.Parent.Parent).StoreID}.{GetType().Name}.{base.Name}.{Text}";

	private static Dictionary<string, Color> knownColorNames
	{
		get
		{
			if (_knownColorNames == null)
			{
				_knownColorNames = new Dictionary<string, Color>();
				_knownColorNames["aqua"] = Color.Aqua;
				_knownColorNames["black"] = Color.Black;
				_knownColorNames["blue"] = Color.Blue;
				_knownColorNames["fuchsia"] = Color.Fuchsia;
				_knownColorNames["gray"] = Color.Gray;
				_knownColorNames["green"] = Color.Green;
				_knownColorNames["lime"] = Color.Lime;
				_knownColorNames["maroon"] = Color.Maroon;
				_knownColorNames["navy"] = Color.Navy;
				_knownColorNames["olive"] = Color.Olive;
				_knownColorNames["orange"] = Color.Orange;
				_knownColorNames["purple"] = Color.Purple;
				_knownColorNames["red"] = Color.Red;
				_knownColorNames["silver"] = Color.Silver;
				_knownColorNames["teal"] = Color.Teal;
				_knownColorNames["white"] = Color.White;
				_knownColorNames["yellow"] = Color.Yellow;
				_knownColorValues = new Dictionary<Color, string>();
				foreach (KeyValuePair<string, Color> knownColorName in _knownColorNames)
				{
					_knownColorValues[knownColorName.Value] = knownColorName.Key;
				}
			}
			return _knownColorNames;
		}
	}

	private static Dictionary<Color, string> knownColorValues
	{
		get
		{
			if (_knownColorValues == null)
			{
				_ = knownColorNames;
			}
			return _knownColorValues;
		}
	}

	public event EventHandler NeedUpdateUI;

	public event EventHandler ColorSelected;

	public SchemesColorEditorUserControl()
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

	private void colorsListView_Click(object sender, EventArgs e)
	{
	}

	private void colorsListView_DoubleClick(object sender, EventArgs e)
	{
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

	private void colorsListView_SizeChanged(object sender, EventArgs e)
	{
		colorsListView.Columns[0].Width = colorsListView.ClientSize.Width - 1;
	}

	private void schemesColorEditorUserControl_Load(object sender, EventArgs e)
	{
		checkEnsureFilled();
		if (_needEnsureVisibleListViewItem != null)
		{
			doSelectItem(_needEnsureVisibleListViewItem);
		}
		else if (externalColorEditorInformationProvider != null)
		{
			schemesComboBox.SelectedIndex = Convert.ToInt32(externalColorEditorInformationProvider.RestorePerUserPerWorkstationValue(storeID + ".SchemesComboBox.SelectedIndex", schemesComboBox.SelectedIndex.ToString()));
		}
		saveState();
		_ignoreComboBoxChanges = false;
		colorsListView_SizeChanged(null, null);
	}

	private void schemesComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!_ignoreComboBoxChanges)
		{
			saveState();
		}
		colorsListView.BeginUpdate();
		_ignoreListViewChanges = true;
		try
		{
			colorsListView.Items.Clear();
			if (schemesComboBox.SelectedItem != null)
			{
				IColorScheme colorScheme = (IColorScheme)schemesComboBox.SelectedItem;
				Color[] colors = colorScheme.Colors;
				if (colors != null && colors.Length > 0)
				{
					List<Color> list = new List<Color>(colors);
					IExternalColorEditorInformationProvider externalColorEditorInformationProvider = this.externalColorEditorInformationProvider;
					foreach (Color item in list)
					{
						string displayText = ConvertColorToHtmlColor(item);
						externalColorEditorInformationProvider?.FormatDisplayText(item, ref displayText);
						ListViewItem listViewItem = new ListViewItem();
						listViewItem.Text = displayText;
						listViewItem.Tag = item;
						colorsListView.Items.Add(listViewItem);
					}
				}
			}
			colorsListView_SizeChanged(null, null);
		}
		finally
		{
			colorsListView.EndUpdate();
			_ignoreListViewChanges = false;
		}
		if (this.NeedUpdateUI != null)
		{
			this.NeedUpdateUI(this, EventArgs.Empty);
		}
	}

	private void saveState()
	{
		if (externalColorEditorInformationProvider != null)
		{
			externalColorEditorInformationProvider.SavePerUserPerWorkstationValue(storeID + ".SchemesComboBox.SelectedIndex", schemesComboBox.SelectedIndex.ToString());
		}
	}

	private void colorsListView_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!_ignoreListViewChanges && this.NeedUpdateUI != null)
		{
			this.NeedUpdateUI(this, EventArgs.Empty);
		}
	}

	public bool ContainsColor(Color value)
	{
		IColorScheme[] array = colorSchemes;
		if (array != null)
		{
			IColorScheme[] array2 = colorSchemes;
			foreach (IColorScheme colorScheme in array2)
			{
				Color[] colors = colorScheme.Colors;
				foreach (Color color in colors)
				{
					if (color == value)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void checkEnsureFilled()
	{
		IColorScheme[] array = colorSchemes;
		if (array != null && schemesComboBox.Items.Count <= 0)
		{
			schemesComboBox.DisplayMember = "Name";
			schemesComboBox.Items.AddRange(array);
			if (schemesComboBox.Items.Count > 0)
			{
				schemesComboBox.SelectedIndex = 0;
			}
		}
	}

	public static bool IsNamedHtmlColor(Color color)
	{
		if (!(color == Color.Transparent))
		{
			return knownColorValues.ContainsKey(color);
		}
		return true;
	}

	public static string GetNamedHtmlColor(Color color)
	{
		if (color == Color.Transparent)
		{
			return "transparent";
		}
		if (IsNamedHtmlColor(color))
		{
			return knownColorValues[color];
		}
		return ConvertColorToHtmlColor(color);
	}

	public static string ConvertColorToHtmlColor(Color color)
	{
		if (color == Color.Transparent)
		{
			return "transparent";
		}
		if (knownColorValues.ContainsKey(color))
		{
			return knownColorValues[color];
		}
		return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
	}

	public static Color? ConvertHtmlColorToColor(string htmlColor)
	{
		if (string.IsNullOrEmpty(htmlColor))
		{
			return null;
		}
		if (htmlColor == "transparent")
		{
			return Color.Transparent;
		}
		if (knownColorNames.ContainsKey(htmlColor))
		{
			return knownColorNames[htmlColor];
		}
		htmlColor = htmlColor.Trim().Trim('#').Trim();
		if (htmlColor.Length != 6)
		{
			return null;
		}
		int red = Convert.ToInt32(htmlColor.Substring(0, 2), 16);
		int green = Convert.ToInt32(htmlColor.Substring(2, 2), 16);
		int blue = Convert.ToInt32(htmlColor.Substring(4, 2), 16);
		return Color.FromArgb(red, green, blue);
	}

	public static Color GetComplementaryColor(Color color)
	{
		return Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
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
		this.schemesComboBox = new System.Windows.Forms.ComboBox();
		base.SuspendLayout();
		this.colorsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.columnHeader1 });
		this.colorsListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.colorsListView.FullRowSelect = true;
		this.colorsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
		this.colorsListView.HideSelection = false;
		this.colorsListView.Location = new System.Drawing.Point(0, 24);
		this.colorsListView.Margin = new System.Windows.Forms.Padding(4);
		this.colorsListView.MultiSelect = false;
		this.colorsListView.Name = "colorsListView";
		this.colorsListView.OwnerDraw = true;
		this.colorsListView.ShowGroups = false;
		this.colorsListView.ShowItemToolTips = true;
		this.colorsListView.Size = new System.Drawing.Size(200, 161);
		this.colorsListView.TabIndex = 1;
		this.colorsListView.UseCompatibleStateImageBehavior = false;
		this.colorsListView.View = System.Windows.Forms.View.Details;
		this.colorsListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(colorsListView_MouseDoubleClick);
		this.colorsListView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(colorsListView_DrawColumnHeader);
		this.colorsListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(colorsListView_DrawItem);
		this.colorsListView.SelectedIndexChanged += new System.EventHandler(colorsListView_SelectedIndexChanged);
		this.colorsListView.SizeChanged += new System.EventHandler(colorsListView_SizeChanged);
		this.colorsListView.DoubleClick += new System.EventHandler(colorsListView_DoubleClick);
		this.colorsListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(colorsListView_DrawSubItem);
		this.colorsListView.Click += new System.EventHandler(colorsListView_Click);
		this.schemesComboBox.Dock = System.Windows.Forms.DockStyle.Top;
		this.schemesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.schemesComboBox.FormattingEnabled = true;
		this.schemesComboBox.Location = new System.Drawing.Point(0, 0);
		this.schemesComboBox.Margin = new System.Windows.Forms.Padding(4);
		this.schemesComboBox.MaxDropDownItems = 20;
		this.schemesComboBox.Name = "schemesComboBox";
		this.schemesComboBox.Size = new System.Drawing.Size(200, 24);
		this.schemesComboBox.TabIndex = 0;
		this.schemesComboBox.SelectedIndexChanged += new System.EventHandler(schemesComboBox_SelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.colorsListView);
		base.Controls.Add(this.schemesComboBox);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "SchemesColorEditorUserControl";
		base.Size = new System.Drawing.Size(200, 185);
		base.Load += new System.EventHandler(schemesColorEditorUserControl_Load);
		base.ResumeLayout(false);
	}
}
