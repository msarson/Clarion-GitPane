using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class FontServicePanel : AbstractOptionPanel
{
	private FontSelectionPanel fontSelectionPanel;

	private ComboBox combo;

	public override void LoadPanelContents()
	{
		FontService.Load();
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.FontServiceOptionsPanel.xfrm"));
		fontSelectionPanel = new FontSelectionPanel();
		fontSelectionPanel.Dock = DockStyle.Fill;
		ControlDictionary["FontGroupBox"].Controls.Add(fontSelectionPanel);
		combo = (ComboBox)ControlDictionary["selectComponentFontComboBox"];
		bool flag = true;
		foreach (ComponentFont fontComponentsDescription in FontService.FontComponentsDescriptions)
		{
			combo.Items.Add(fontComponentsDescription);
			if (flag)
			{
				flag = false;
				combo.SelectedItem = fontComponentsDescription;
				fontSelectionPanel.CurrentFont = fontComponentsDescription.Font;
			}
		}
		if (combo.Items.Count > 0)
		{
			combo.SelectedIndexChanged += combo_SelectedIndexChanged;
			fontSelectionPanel.FontSelectedChanged += fontSelectionPanel_FontSelectedChanged;
		}
		else
		{
			ControlDictionary["FontGroupBox"].Enabled = false;
		}
	}

	private void fontSelectionPanel_FontSelectedChanged(object sender, EventArgs e)
	{
		ComponentFont componentFont = combo.SelectedItem as ComponentFont;
		componentFont.Font = fontSelectionPanel.CurrentFont;
	}

	private void combo_SelectedIndexChanged(object sender, EventArgs e)
	{
		ComponentFont componentFont = combo.SelectedItem as ComponentFont;
		fontSelectionPanel.CurrentFont = componentFont.Font;
	}

	public override bool StorePanelContents()
	{
		List<ComponentFont> list = new List<ComponentFont>();
		foreach (object item2 in combo.Items)
		{
			ComponentFont item = item2 as ComponentFont;
			list.Add(item);
		}
		FontService.Save(list.ToArray());
		return true;
	}
}
