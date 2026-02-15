using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SearchAndReplace;

public class SearchRootNode : ExtTreeNode
{
	private List<SearchResult> results;

	private string pattern;

	private int fileCount;

	public List<SearchResult> Results => results;

	public SearchRootNode(string pattern, List<SearchResult> results, int fileCount)
	{
		drawDefault = false;
		this.results = results;
		this.pattern = pattern;
		this.fileCount = fileCount;
		base.Text = GetText();
	}

	public static string GetOccurencesString(int count)
	{
		if (count == 1)
		{
			return StringParser.Parse("${res:MainWindow.Windows.SearchResultPanel.OneOccurrence}");
		}
		return StringParser.Parse("${res:MainWindow.Windows.SearchResultPanel.OccurrencesCount}", new string[1, 2] { 
		{
			"Count",
			count.ToString()
		} });
	}

	public static string GetFileCountString(int count)
	{
		if (count == 1)
		{
			return StringParser.Parse("${res:MainWindow.Windows.SearchResultPanel.OneFile}");
		}
		return StringParser.Parse("${res:MainWindow.Windows.SearchResultPanel.FileCount}", new string[1, 2] { 
		{
			"Count",
			count.ToString()
		} });
	}

	private string GetText()
	{
		return StringParser.Parse("${res:MainWindow.Windows.SearchResultPanel.OccurrencesOf}", new string[1, 2] { { "Pattern", pattern } }) + " (" + GetOccurencesString(results.Count) + StringParser.Parse(" ${res:MainWindow.Windows.SearchResultPanel.In} ") + GetFileCountString(fileCount) + ")";
	}

	protected override int MeasureItemWidth(DrawTreeNodeEventArgs e)
	{
		return MeasureTextWidth(e.Graphics, GetText(), ExtTreeNode.BoldBigFont);
	}

	protected override void DrawForeground(DrawTreeNodeEventArgs e, float x)
	{
		DrawForegroundExpandImg(e, ref x);
		DrawText(e, GetText(), SystemBrushes.WindowText, ExtTreeNode.BoldBigFont, ref x);
	}
}
