using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace ICSharpCode.SharpDevelop.Gui;

public class CommonAboutDialog : XmlForm
{
	public ScrollBox ScrollBox => (ScrollBox)base.ControlDictionary["aboutPictureScrollBox"];

	public CommonAboutDialog()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.CommonAboutDialog.xfrm"));
	}

	protected override void SetupXmlLoader()
	{
		xmlLoader.StringValueFilter = new SharpDevelopStringValueFilter();
		xmlLoader.PropertyValueCreator = new SharpDevelopPropertyValueCreator();
	}
}
