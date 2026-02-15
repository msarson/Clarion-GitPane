using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace ICSharpCode.SharpDevelop.Gui;

public class FontSelectionPanel : BaseSharpDevelopUserControl
{
	private FontSelectionPanelHelper helper;

	public string CurrentFontString
	{
		get
		{
			return CurrentFont.ToString();
		}
		set
		{
			CurrentFont = ParseFont(value);
		}
	}

	public Font CurrentFont
	{
		get
		{
			if (helper == null)
			{
				return null;
			}
			return helper.GetSelectedFont();
		}
		set
		{
			if (helper == null)
			{
				helper = new FontSelectionPanelHelper((ComboBox)ControlDictionary["fontSizeComboBox"], (ComboBox)ControlDictionary["fontListComboBox"], value);
				helper.StartThread();
				((ComboBox)ControlDictionary["fontListComboBox"]).MeasureItem += helper.MeasureComboBoxItem;
				((ComboBox)ControlDictionary["fontListComboBox"]).DrawItem += helper.ComboBoxDrawItem;
			}
			else
			{
				int selectedIndex = 0;
				for (int i = 0; i < ((ComboBox)ControlDictionary["fontListComboBox"]).Items.Count; i++)
				{
					FontSelectionPanelHelper.FontDescriptor fontDescriptor = (FontSelectionPanelHelper.FontDescriptor)((ComboBox)ControlDictionary["fontListComboBox"]).Items[i];
					if (fontDescriptor.Name == value.Name)
					{
						selectedIndex = i;
					}
				}
				((ComboBox)ControlDictionary["fontListComboBox"]).SelectedIndex = selectedIndex;
			}
			((ComboBox)ControlDictionary["fontSizeComboBox"]).Text = value.Size.ToString();
		}
	}

	public event EventHandler FontSelectedChanged;

	public FontSelectionPanel()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.FontSelectionPanel.xfrm"));
		for (int i = 6; i <= 24; i++)
		{
			((ComboBox)ControlDictionary["fontSizeComboBox"]).Items.Add(i);
		}
		((ComboBox)ControlDictionary["fontSizeComboBox"]).TextChanged += UpdateFontPreviewLabel;
		((ComboBox)ControlDictionary["fontSizeComboBox"]).Enabled = false;
		((ComboBox)ControlDictionary["fontListComboBox"]).Enabled = false;
		((ComboBox)ControlDictionary["fontListComboBox"]).TextChanged += UpdateFontPreviewLabel;
		((ComboBox)ControlDictionary["fontListComboBox"]).SelectedIndexChanged += UpdateFontPreviewLabel;
	}

	public static Font ParseFont(string font)
	{
		try
		{
			string[] array = font.Split(',', '=');
			return new Font(array[1], float.Parse(array[3]));
		}
		catch (Exception message)
		{
			LoggingService.Warn(message);
			return ResourceService.DefaultMonospacedFont;
		}
	}

	private void UpdateFontPreviewLabel(object sender, EventArgs e)
	{
		helper.UpdateFontPreviewLabel(ControlDictionary["fontPreviewLabel"]);
		if (this.FontSelectedChanged != null)
		{
			this.FontSelectedChanged(this, EventArgs.Empty);
		}
	}
}
