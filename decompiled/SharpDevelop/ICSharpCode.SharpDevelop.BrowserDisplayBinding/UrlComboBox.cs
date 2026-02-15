using System;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class UrlComboBox : AbstractComboBoxCommand
{
	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		ToolBarComboBox toolBarComboBox = (ToolBarComboBox)Owner;
		toolBarComboBox.ComboBox.Width *= 3;
		((HtmlViewPane)toolBarComboBox.Caller).SetUrlComboBox(toolBarComboBox.ComboBox);
	}
}
