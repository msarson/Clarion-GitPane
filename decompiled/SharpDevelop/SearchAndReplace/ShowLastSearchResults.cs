using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class ShowLastSearchResults : AbstractMenuCommand
{
	private ToolBarDropDownButton dropDownButton;

	public override bool IsEnabled => SearchInFilesManager.LastSearches.Count > 0;

	public override void Run()
	{
	}

	private void SwitchSearchResults(object sender, EventArgs e)
	{
		SearchAllFinishedEventArgs e2 = (SearchAllFinishedEventArgs)((ToolStripItem)sender).Tag;
		SearchInFilesManager.LastSearches.Remove(e2);
		SearchInFilesManager.LastSearches.Insert(0, e2);
		UpdateLastSearches(null, e2);
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(SearchResultPanel));
		if (pad != null)
		{
			pad.BringPadToFront();
			SearchResultPanel.Instance.ShowSearchResults(e2.Pattern, e2.Results);
		}
		else
		{
			MessageService.ShowError("SearchResultPanel can't be found.");
		}
	}

	private void ClearHistory(object sender, EventArgs e)
	{
		SearchResultPanel.Instance.Clear();
		SearchInFilesManager.LastSearches.Clear();
		UpdateLastSearches(null, null);
	}

	private void UpdateLastSearches(object sender, SearchAllFinishedEventArgs e)
	{
		dropDownButton.DropDownItems.Clear();
		foreach (SearchAllFinishedEventArgs lastSearch in SearchInFilesManager.LastSearches)
		{
			ToolStripItem toolStripItem = new ToolStripMenuItem();
			toolStripItem.Text = StringParser.Parse("${res:MainWindow.Windows.SearchResultPanel.OccurrencesOf}", new string[1, 2] { { "Pattern", lastSearch.Pattern } }) + " (" + SearchRootNode.GetOccurencesString(lastSearch.Results.Count) + ")";
			toolStripItem.Tag = lastSearch;
			toolStripItem.Click += SwitchSearchResults;
			dropDownButton.DropDownItems.Add(toolStripItem);
		}
		dropDownButton.DropDownItems.Add(new ToolStripSeparator());
		ToolStripItem toolStripItem2 = new ToolStripMenuItem();
		toolStripItem2.Text = StringParser.Parse("${res:MainWindow.Windows.SearchResultPanel.ClearHistory}");
		toolStripItem2.Click += ClearHistory;
		dropDownButton.DropDownItems.Add(toolStripItem2);
		dropDownButton.Enabled = IsEnabled;
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		dropDownButton = (ToolBarDropDownButton)Owner;
		SearchInFilesManager.SearchAllFinished += UpdateLastSearches;
		UpdateLastSearches(null, null);
	}
}
