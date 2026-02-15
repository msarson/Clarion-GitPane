using System;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ClassBrowserSearchTerm : AbstractComboBoxCommand
{
	private ComboBox comboBox;

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		ToolBarComboBox toolBarComboBox = (ToolBarComboBox)Owner;
		comboBox = toolBarComboBox.ComboBox;
		comboBox.DropDownStyle = ComboBoxStyle.DropDown;
		comboBox.TextChanged += ComboBoxTextChanged;
	}

	private void ComboBoxTextChanged(object sender, EventArgs e)
	{
		ClassBrowserPad.Instance.SearchTerm = comboBox.Text;
		Run();
	}

	public override void Run()
	{
		ClassBrowserPad.Instance.StartSearch();
	}
}
