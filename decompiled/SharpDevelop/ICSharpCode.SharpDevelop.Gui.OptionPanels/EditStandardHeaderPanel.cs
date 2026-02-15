using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class EditStandardHeaderPanel : AbstractOptionPanel
{
	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.EditStandardHeaderPanel.xfrm"));
		ControlDictionary["headerTextBox"].Font = ResourceService.DefaultMonospacedFont;
		foreach (StandardHeader standardHeader in StandardHeader.StandardHeaders)
		{
			((ComboBox)ControlDictionary["headerChooser"]).Items.Add(standardHeader);
		}
		((ComboBox)ControlDictionary["headerChooser"]).SelectedIndexChanged += SelectedIndexChanged;
		((ComboBox)ControlDictionary["headerChooser"]).SelectedIndex = 0;
		((TextBox)ControlDictionary["headerTextBox"]).TextChanged += TextChangedEvent;
	}

	private void TextChangedEvent(object sender, EventArgs e)
	{
		((StandardHeader)((ComboBox)ControlDictionary["headerChooser"]).SelectedItem).Header = ControlDictionary["headerTextBox"].Text;
	}

	private void SelectedIndexChanged(object sender, EventArgs e)
	{
		((TextBox)ControlDictionary["headerTextBox"]).TextChanged -= TextChangedEvent;
		int selectedIndex = ((ComboBox)ControlDictionary["headerChooser"]).SelectedIndex;
		if (selectedIndex >= 0)
		{
			ControlDictionary["headerTextBox"].Text = ((StandardHeader)((ComboBox)ControlDictionary["headerChooser"]).SelectedItem).Header;
			ControlDictionary["headerTextBox"].Enabled = true;
		}
		else
		{
			ControlDictionary["headerTextBox"].Text = "";
			ControlDictionary["headerTextBox"].Enabled = false;
		}
		((TextBox)ControlDictionary["headerTextBox"]).TextChanged += TextChangedEvent;
	}

	public override bool StorePanelContents()
	{
		StandardHeader.StoreHeaders();
		return true;
	}
}
