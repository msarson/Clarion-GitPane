using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class OutputWindowOptionsPanel : AbstractOptionPanel
{
	public static readonly string OutputWindowsProperty = "SharpDevelop.UI.OutputWindowOptions";

	private FontSelectionPanel fontSelectionPanel;

	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.OutputWindowOptionsPanel.xfrm"));
		Properties properties = PropertyService.Get(OutputWindowsProperty, new Properties());
		fontSelectionPanel = new FontSelectionPanel();
		fontSelectionPanel.Dock = DockStyle.Fill;
		ControlDictionary["FontGroupBox"].Controls.Add(fontSelectionPanel);
		((CheckBox)ControlDictionary["wordWrapCheckBox"]).Checked = properties.Get("WordWrap", defaultValue: true);
		fontSelectionPanel.CurrentFont = FontService.GetFont(FontService.FontType.TextEditor);
	}

	public override bool StorePanelContents()
	{
		Properties properties = PropertyService.Get(OutputWindowsProperty, new Properties());
		properties.Set("WordWrap", ((CheckBox)ControlDictionary["wordWrapCheckBox"]).Checked);
		FontService.SetFont(FontService.FontType.TextEditor, fontSelectionPanel.CurrentFont);
		PropertyService.Set(OutputWindowsProperty, properties);
		return true;
	}
}
