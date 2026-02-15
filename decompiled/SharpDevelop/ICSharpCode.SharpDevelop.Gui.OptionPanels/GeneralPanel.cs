using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class GeneralPanel : AbstractOptionPanel
{
	private const string _OneInstanceCheckBox = "OneInstanceCheckBox";

	private const string _UseRedirectionFileOpenCheckBox = "UseRedirectionFileOpenCheckBox";

	private const string _ShowNotificationsCheckBox = "ShowNotificationsCheckBox";

	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.GeneralIDEOptionsPanel.xfrm"));
		((CheckBox)ControlDictionary["UseRedirectionFileOpenCheckBox"]).Checked = PropertyService.Get("UseRedirectionFileOpen", true, "FileDialog");
		((CheckBox)ControlDictionary["OneInstanceCheckBox"]).Checked = !WorkbenchSingleton.SupportMultipleInstances;
		((CheckBox)ControlDictionary["OneInstanceCheckBox"]).Checked = !WorkbenchSingleton.SupportMultipleInstances;
		((CheckBox)ControlDictionary["ShowNotificationsCheckBox"]).Checked = PropertyService.Get("ShowNotification", defaultValue: false);
	}

	public override bool StorePanelContents()
	{
		WorkbenchSingleton.SupportMultipleInstances = !((CheckBox)ControlDictionary["OneInstanceCheckBox"]).Checked;
		PropertyService.Set("UseRedirectionFileOpen", ((CheckBox)ControlDictionary["UseRedirectionFileOpenCheckBox"]).Checked, "FileDialog");
		PropertyService.Set("ShowNotification", ((CheckBox)ControlDictionary["ShowNotificationsCheckBox"]).Checked);
		return true;
	}
}
