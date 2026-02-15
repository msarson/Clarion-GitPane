using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class SearchResultPanel : AbstractPadContent, IOwnerState, IClipboardHandler
{
	private enum SearchResultPanelOwnerState
	{
		DefaultMode = 1,
		SpecialPanelMode
	}

	private static SearchResultPanel instance;

	private Panel myPanel = new Panel();

	private ExtTreeView resultTreeView = new ExtTreeView();

	private ToolStrip toolStrip;

	private string curPattern;

	private List<SearchResult> curResults;

	private Control specialPanel;

	public static SearchResultPanel Instance => instance;

	public override Control Control => myPanel;

	public SearchResultPanelViewMode ViewMode
	{
		get
		{
			return PropertyService.Get("SearchAndReplace.SearchResultPanelViewMode", SearchResultPanelViewMode.Flat);
		}
		set
		{
			PropertyService.Set("SearchAndReplace.SearchResultPanelViewMode", value);
			ShowSearchResults(curPattern, curResults);
		}
	}

	public int ResultsCount
	{
		get
		{
			if (curResults != null)
			{
				return curResults.Count;
			}
			return 0;
		}
	}

	Enum IOwnerState.InternalState => (specialPanel == null) ? SearchResultPanelOwnerState.DefaultMode : SearchResultPanelOwnerState.SpecialPanelMode;

	public bool EnableCut => false;

	public bool EnableCopy => resultTreeView.Nodes.Count > 0;

	public bool EnablePaste => false;

	public bool EnableDelete => false;

	public bool EnableSelectAll => false;

	public void ExpandAll()
	{
		Stack<TreeNode> stack = new Stack<TreeNode>();
		foreach (TreeNode node in resultTreeView.Nodes)
		{
			stack.Push(node);
		}
		while (stack.Count > 0)
		{
			TreeNode treeNode = stack.Pop();
			treeNode.Expand();
			foreach (TreeNode node2 in treeNode.Nodes)
			{
				stack.Push(node2);
			}
		}
	}

	public void Clear()
	{
		resultTreeView.Nodes.Clear();
		ToolbarService.UpdateToolbar(toolStrip);
	}

	public void CollapseAll()
	{
		Stack<TreeNode> stack = new Stack<TreeNode>();
		foreach (TreeNode node in resultTreeView.Nodes)
		{
			stack.Push(node);
		}
		while (stack.Count > 0)
		{
			TreeNode treeNode = stack.Pop();
			treeNode.Collapse();
			foreach (TreeNode node2 in treeNode.Nodes)
			{
				stack.Push(node2);
			}
		}
	}

	private void CopyAll()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (TreeNode node in resultTreeView.Nodes)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(node.Text);
			foreach (TreeNode node2 in node.Nodes)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(node2.Text);
				if (ViewMode != SearchResultPanelViewMode.PerFile)
				{
					continue;
				}
				foreach (TreeNode node3 in node2.Nodes)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(node3.Text);
				}
			}
		}
		ClipboardWrapper.SetText(stringBuilder.ToString());
	}

	private void ShowSearchResultsPerFile()
	{
		Dictionary<string, SearchFolderNode> dictionary = new Dictionary<string, SearchFolderNode>();
		foreach (SearchResult curResult in curResults)
		{
			if (!dictionary.ContainsKey(curResult.FileName))
			{
				dictionary[curResult.FileName] = new SearchFolderNode(curResult.FileName);
			}
			dictionary[curResult.FileName].Results.Add(curResult);
		}
		SearchRootNode searchRootNode = new SearchRootNode(curPattern, curResults, dictionary.Count);
		foreach (SearchFolderNode value in dictionary.Values)
		{
			value.SetText();
			searchRootNode.Nodes.Add(value);
		}
		resultTreeView.Nodes.Add(searchRootNode);
		searchRootNode.Expand();
	}

	private void ShowSearchResultsFlat()
	{
		Dictionary<string, SearchFolderNode> dictionary = new Dictionary<string, SearchFolderNode>();
		foreach (SearchResult curResult in curResults)
		{
			if (!dictionary.ContainsKey(curResult.FileName))
			{
				dictionary[curResult.FileName] = new SearchFolderNode(curResult.FileName);
			}
			dictionary[curResult.FileName].Results.Add(curResult);
		}
		SearchRootNode searchRootNode = new SearchRootNode(curPattern, curResults, dictionary.Count);
		foreach (SearchFolderNode value in dictionary.Values)
		{
			value.PerformInitialization();
			foreach (SearchResultNode node in value.Nodes)
			{
				node.ShowFileName = true;
				searchRootNode.Nodes.Add(node);
			}
		}
		resultTreeView.Nodes.Add(searchRootNode);
		searchRootNode.Expand();
	}

	public void GoToFirstResultIfUnique()
	{
		if (ResultsCount == 1)
		{
			SearchResult searchResult = curResults[0];
			IDocument document = searchResult.CreateDocument();
			TextLocation startPosition = searchResult.GetStartPosition(document);
			FileService.JumpToFilePosition(searchResult.FileName, startPosition.Y, startPosition.X);
		}
	}

	public void ShowSearchResults(string pattern, List<SearchResult> results)
	{
		RemoveSpecialPanel();
		curPattern = pattern;
		curResults = results;
		if (results != null)
		{
			resultTreeView.BeginUpdate();
			resultTreeView.Nodes.Clear();
			switch (ViewMode)
			{
			case SearchResultPanelViewMode.PerFile:
				ShowSearchResultsPerFile();
				break;
			case SearchResultPanelViewMode.Flat:
				ShowSearchResultsFlat();
				break;
			}
			resultTreeView.EndUpdate();
			ToolbarService.UpdateToolbar(toolStrip);
		}
	}

	public SearchResultPanel()
	{
		instance = this;
		resultTreeView.Dock = DockStyle.Fill;
		resultTreeView.Font = FontService.GetFont(FontService.FontType.ListControls);
		resultTreeView.IsSorted = false;
		toolStrip = ToolbarService.CreateToolStrip(this, "/SharpDevelop/Pads/SearchResultPanel/Toolbar");
		toolStrip.Stretch = true;
		toolStrip.GripStyle = ToolStripGripStyle.Hidden;
		myPanel.Controls.AddRange(new Control[2] { resultTreeView, toolStrip });
	}

	public void ShowSpecialPanel(Control ctl)
	{
		ctl.Dock = DockStyle.Fill;
		if (specialPanel != ctl)
		{
			if (specialPanel != null)
			{
				myPanel.Controls.Remove(specialPanel);
			}
			else
			{
				myPanel.Controls.Remove(resultTreeView);
			}
			specialPanel = ctl;
			myPanel.Controls.Add(ctl);
			myPanel.Controls.SetChildIndex(ctl, 0);
			ToolbarService.UpdateToolbar(toolStrip);
		}
	}

	public void RemoveSpecialPanel()
	{
		if (specialPanel != null)
		{
			specialPanel = null;
			myPanel.Controls.Remove(specialPanel);
			myPanel.Controls.Add(resultTreeView);
			myPanel.Controls.SetChildIndex(resultTreeView, 0);
			ToolbarService.UpdateToolbar(toolStrip);
		}
	}

	public void Cut()
	{
	}

	public void Copy()
	{
		ExpandAll();
		CopyAll();
	}

	public void Paste()
	{
	}

	public void Delete()
	{
	}

	public void SelectAll()
	{
	}
}
