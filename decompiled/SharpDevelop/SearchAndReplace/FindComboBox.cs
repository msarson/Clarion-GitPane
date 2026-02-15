using System;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace SearchAndReplace;

public class FindComboBox : AbstractComboBoxCommand
{
	private ComboBox comboBox;

	private void RefreshComboBox()
	{
		comboBox.Items.Clear();
		string[] findPatterns = SearchOptions.FindPatterns;
		foreach (string item in findPatterns)
		{
			comboBox.Items.Add(item);
		}
		comboBox.Text = SearchOptions.FindPattern;
	}

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			CommitSearch();
			e.Handled = true;
		}
	}

	private void CommitSearch()
	{
		if (comboBox.Text.Length > 0)
		{
			LoggingService.Debug("FindComboBox.CommitSearch()");
			SearchOptions.SearchAndReplaceBinding = SearchOptions.CurrentDocumentBinding;
			SearchOptions.FindPattern = comboBox.Text;
			SearchReplaceManager.FindNext(null);
			comboBox.Focus();
		}
	}

	private void SearchOptionsChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.Key == "FindPatterns")
		{
			RefreshComboBox();
		}
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		ToolBarComboBox toolBarComboBox = (ToolBarComboBox)Owner;
		comboBox = toolBarComboBox.ComboBox;
		comboBox.DropDownStyle = ComboBoxStyle.DropDown;
		comboBox.KeyPress += OnKeyPress;
		SearchOptions.Properties.PropertyChanged += SearchOptionsChanged;
		RefreshComboBox();
	}
}
