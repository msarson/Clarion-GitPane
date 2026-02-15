using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

internal class FontSelectionPanelHelper
{
	public class FontDescriptor
	{
		private FontFamily fontFamily;

		internal string Name;

		internal bool IsMonospaced;

		public FontDescriptor(FontFamily fontFamily)
		{
			this.fontFamily = fontFamily;
			Name = fontFamily.Name;
		}

		internal void DetectMonospaced(Graphics g)
		{
			IsMonospaced = DetectMonospaced(g, fontFamily);
		}

		private static bool DetectMonospaced(Graphics g, FontFamily fontFamily)
		{
			using Font font = new Font(fontFamily, 10f);
			int width = TextRenderer.MeasureText("i.", font).Width;
			int width2 = TextRenderer.MeasureText("mw", font).Width;
			return width == width2;
		}
	}

	private ComboBox fontSizeComboBox;

	private ComboBox fontListComboBox;

	private Font defaultFont;

	private static StringFormat drawStringFormat = new StringFormat(StringFormatFlags.NoWrap);

	private Font boldComboBoxFont;

	public FontSelectionPanelHelper(ComboBox fontSizeComboBox, ComboBox fontListComboBox, Font defaultFont)
	{
		this.fontSizeComboBox = fontSizeComboBox;
		this.fontListComboBox = fontListComboBox;
		this.defaultFont = defaultFont;
		boldComboBoxFont = new Font(fontListComboBox.Font, FontStyle.Bold);
	}

	public void StartThread()
	{
		Thread thread = new Thread(DetectMonospacedThread);
		thread.IsBackground = true;
		thread.Start();
	}

	private void DetectMonospacedThread()
	{
		Thread.Sleep(0);
		InstalledFontCollection installedFontCollection = new InstalledFontCollection();
		Font currentFont = defaultFont;
		List<FontDescriptor> fonts = new List<FontDescriptor>();
		int index = 0;
		FontFamily[] families = installedFontCollection.Families;
		foreach (FontFamily fontFamily in families)
		{
			if (fontFamily.IsStyleAvailable(FontStyle.Regular) && fontFamily.IsStyleAvailable(FontStyle.Bold) && fontFamily.IsStyleAvailable(FontStyle.Italic))
			{
				if (fontFamily.Name == currentFont.Name)
				{
					index = fonts.Count;
				}
				fonts.Add(new FontDescriptor(fontFamily));
			}
		}
		WorkbenchSingleton.SafeThreadAsyncCall(delegate
		{
			fontListComboBox.Items.AddRange(fonts.ToArray());
			fontSizeComboBox.Enabled = true;
			fontListComboBox.Enabled = true;
			fontListComboBox.SelectedIndex = index;
			fontSizeComboBox.Text = currentFont.Size.ToString();
		});
		using (Bitmap image = new Bitmap(1, 1))
		{
			using Graphics g = Graphics.FromImage(image);
			foreach (FontDescriptor item in fonts)
			{
				item.DetectMonospaced(g);
			}
		}
		fontListComboBox.Invalidate();
	}

	internal void MeasureComboBoxItem(object sender, MeasureItemEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (e.Index >= 0)
		{
			FontDescriptor fontDescriptor = (FontDescriptor)comboBox.Items[e.Index];
			e.ItemWidth = (int)e.Graphics.MeasureString(fontDescriptor.Name, comboBox.Font).Width;
			e.ItemHeight = comboBox.Font.Height;
		}
	}

	internal void ComboBoxDrawItem(object sender, DrawItemEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		e.DrawBackground();
		Rectangle rectangle = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
		Brush brush = SystemBrushes.WindowText;
		if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
		{
			brush = SystemBrushes.HighlightText;
		}
		if (!comboBox.Enabled)
		{
			e.Graphics.DrawString(ResourceService.GetString("ICSharpCode.SharpDevelop.Gui.Pads.ClassScout.LoadingNode"), comboBox.Font, brush, rectangle, drawStringFormat);
		}
		else if (e.Index >= 0)
		{
			FontDescriptor fontDescriptor = (FontDescriptor)comboBox.Items[e.Index];
			e.Graphics.DrawString(fontDescriptor.Name, fontDescriptor.IsMonospaced ? boldComboBoxFont : comboBox.Font, brush, rectangle, drawStringFormat);
		}
		e.DrawFocusRectangle();
	}

	public Font GetSelectedFont()
	{
		if (!fontListComboBox.Enabled)
		{
			return null;
		}
		float emSize = 10f;
		try
		{
			emSize = Math.Max(6f, float.Parse(fontSizeComboBox.Text));
		}
		catch (Exception)
		{
		}
		FontDescriptor fontDescriptor = (FontDescriptor)fontListComboBox.Items[fontListComboBox.SelectedIndex];
		return new Font(fontDescriptor.Name, emSize);
	}

	public void UpdateFontPreviewLabel(Control fontPreviewLabel)
	{
		Font selectedFont = GetSelectedFont();
		fontPreviewLabel.Visible = selectedFont != null;
		if (selectedFont != null)
		{
			fontPreviewLabel.Font = selectedFont;
		}
	}
}
