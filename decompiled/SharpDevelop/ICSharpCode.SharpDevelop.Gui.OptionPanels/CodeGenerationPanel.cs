using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class CodeGenerationPanel : AbstractOptionPanel
{
	private static readonly string codeGenerationProperty = "SharpDevelop.UI.CodeGenerationOptions";

	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.CodeGenerationOptionsPanel.xfrm"));
		Properties properties = PropertyService.Get(codeGenerationProperty, new Properties());
		((CheckBox)ControlDictionary["generateAdditonalCommentsCheckBox"]).Checked = properties.Get("GenerateAdditionalComments", defaultValue: true);
		((CheckBox)ControlDictionary["generateDocCommentsCheckBox"]).Checked = properties.Get("GenerateDocumentComments", defaultValue: true);
		((CheckBox)ControlDictionary["useFullTypeNamesCheckBox"]).Checked = properties.Get("UseFullyQualifiedNames", defaultValue: true);
		((CheckBox)ControlDictionary["blankLinesBetweenMemberCheckBox"]).Checked = properties.Get("BlankLinesBetweenMembers", defaultValue: true);
		((CheckBox)ControlDictionary["elseOnClosingCheckbox"]).Checked = properties.Get("ElseOnClosing", defaultValue: true);
		((CheckBox)ControlDictionary["startBlockOnTheSameLineCheckBox"]).Checked = properties.Get("StartBlockOnSameLine", defaultValue: true);
	}

	public override bool StorePanelContents()
	{
		Properties properties = PropertyService.Get(codeGenerationProperty, new Properties());
		properties.Set("GenerateAdditionalComments", ((CheckBox)ControlDictionary["generateAdditonalCommentsCheckBox"]).Checked);
		properties.Set("GenerateDocumentComments", ((CheckBox)ControlDictionary["generateDocCommentsCheckBox"]).Checked);
		properties.Set("UseFullyQualifiedNames", ((CheckBox)ControlDictionary["useFullTypeNamesCheckBox"]).Checked);
		properties.Set("BlankLinesBetweenMembers", ((CheckBox)ControlDictionary["blankLinesBetweenMemberCheckBox"]).Checked);
		properties.Set("ElseOnClosing", ((CheckBox)ControlDictionary["elseOnClosingCheckbox"]).Checked);
		properties.Set("StartBlockOnSameLine", ((CheckBox)ControlDictionary["startBlockOnTheSameLineCheckBox"]).Checked);
		PropertyService.Set(codeGenerationProperty, properties);
		return true;
	}
}
