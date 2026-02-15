using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Commands;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace ICSharpCode.SharpDevelop.Gui;

public class SortOptionsDialog : BaseSharpDevelopForm
{
	public static readonly string removeDupesOption = "ICSharpCode.SharpDevelop.Gui.SortOptionsDialog.RemoveDuplicateLines";

	public static readonly string caseSensitiveOption = "ICSharpCode.SharpDevelop.Gui.SortOptionsDialog.CaseSensitive";

	public static readonly string ignoreWhiteSpacesOption = "ICSharpCode.SharpDevelop.Gui.SortOptionsDialog.IgnoreWhitespaces";

	public static readonly string sortDirectionOption = "ICSharpCode.SharpDevelop.Gui.SortOptionsDialog.SortDirection";

	public SortOptionsDialog()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.SortOptionsDialog.xfrm"));
		base.AcceptButton = (Button)base.ControlDictionary["okButton"];
		base.CancelButton = (Button)base.ControlDictionary["cancelButton"];
		((CheckBox)base.ControlDictionary["removeDupesCheckBox"]).Checked = PropertyService.Get(removeDupesOption, defaultValue: false);
		((CheckBox)base.ControlDictionary["caseSensitiveCheckBox"]).Checked = PropertyService.Get(caseSensitiveOption, defaultValue: true);
		((CheckBox)base.ControlDictionary["ignoreWhiteSpacesCheckBox"]).Checked = PropertyService.Get(ignoreWhiteSpacesOption, defaultValue: false);
		((RadioButton)base.ControlDictionary["ascendingRadioButton"]).Checked = PropertyService.Get(sortDirectionOption, SortSelection.SortDirection.Ascending) == SortSelection.SortDirection.Ascending;
		((RadioButton)base.ControlDictionary["descendingRadioButton"]).Checked = PropertyService.Get(sortDirectionOption, SortSelection.SortDirection.Ascending) == SortSelection.SortDirection.Descending;
		base.ControlDictionary["okButton"].Click += OkEvent;
	}

	private void OkEvent(object sender, EventArgs e)
	{
		PropertyService.Set(removeDupesOption, ((CheckBox)base.ControlDictionary["removeDupesCheckBox"]).Checked);
		PropertyService.Set(caseSensitiveOption, ((CheckBox)base.ControlDictionary["caseSensitiveCheckBox"]).Checked);
		PropertyService.Set(ignoreWhiteSpacesOption, ((CheckBox)base.ControlDictionary["ignoreWhiteSpacesCheckBox"]).Checked);
		if (((RadioButton)base.ControlDictionary["ascendingRadioButton"]).Checked)
		{
			PropertyService.Set(sortDirectionOption, SortSelection.SortDirection.Ascending);
		}
		else
		{
			PropertyService.Set(sortDirectionOption, SortSelection.SortDirection.Descending);
		}
	}
}
