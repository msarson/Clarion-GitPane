using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class SelectViewMode : AbstractMenuCommand
{
	private ToolBarDropDownButton dropDownButton;

	public override void Run()
	{
	}

	private void SetViewMode(object sender, EventArgs e)
	{
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(SearchResultPanel));
		if (pad != null)
		{
			pad.BringPadToFront();
			SearchResultPanel.Instance.ViewMode = (SearchResultPanelViewMode)((ToolStripItem)sender).Tag;
			UpdateDropDownItems();
		}
		else
		{
			MessageService.ShowError("SearchResultPanel can't be found.");
		}
	}

	private void UpdateDropDownItems()
	{
		foreach (ToolStripItem dropDownItem in dropDownButton.DropDownItems)
		{
			((ToolStripMenuItem)dropDownItem).Checked = (SearchResultPanelViewMode)dropDownItem.Tag == SearchResultPanel.Instance.ViewMode;
		}
	}

	private void GenerateDropDownItems()
	{
		ToolStripMenuItem toolStripMenuItem = null;
		_ = string.Empty;
		foreach (SearchResultPanelViewMode value in Enum.GetValues(typeof(SearchResultPanelViewMode)))
		{
			toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem.Text = StringParser.Parse("${res:MainWindow.Windows.SearchResultPanel." + value.ToString() + "}");
			toolStripMenuItem.Tag = value;
			toolStripMenuItem.Click += SetViewMode;
			toolStripMenuItem.Checked = SearchResultPanel.Instance.ViewMode == value;
			dropDownButton.DropDownItems.Add(toolStripMenuItem);
		}
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		dropDownButton = (ToolBarDropDownButton)Owner;
		GenerateDropDownItems();
	}
}
