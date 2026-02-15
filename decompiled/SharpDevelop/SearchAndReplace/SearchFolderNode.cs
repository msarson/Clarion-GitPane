using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class SearchFolderNode : ExtFolderNode
{
	private List<SearchResult> results = new List<SearchResult>();

	private string fileName;

	private string occurences;

	private Image icon;

	public List<SearchResult> Results => results;

	public SearchFolderNode(string fileName)
	{
		drawDefault = false;
		this.fileName = fileName;
		icon = IconService.GetBitmap(IconService.GetImageForFile(fileName));
		base.Nodes.Add(new TreeNode());
	}

	public void SetText()
	{
		if (results.Count == 1)
		{
			occurences = " (1 occurence)";
		}
		else
		{
			occurences = " (" + results.Count + " occurences)";
		}
		base.Text = fileName + occurences;
	}

	protected override int MeasureItemWidth(DrawTreeNodeEventArgs e)
	{
		Graphics graphics = e.Graphics;
		int num = MeasureTextWidth(graphics, fileName, ExtTreeNode.RegularBigFont);
		num += MeasureTextWidth(graphics, occurences, ExtTreeNode.ItalicBigFont);
		if (icon != null)
		{
			num += icon.Width;
		}
		return num + 3;
	}

	protected override void DrawForeground(DrawTreeNodeEventArgs e, float x)
	{
		DrawForegroundExpandImg(e, ref x);
		DrawForegroundIcon(e, icon, ref x);
		DrawText(e, fileName, SystemBrushes.WindowText, ExtTreeNode.RegularBigFont, ref x);
		DrawText(e, occurences, SystemBrushes.GrayText, ExtTreeNode.ItalicBigFont, ref x);
	}

	protected override void Initialize()
	{
		base.Nodes.Clear();
		IDocument document = results[0].CreateDocument();
		if (document.HighlightingStrategy == null)
		{
			document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategyForFile(fileName);
		}
		foreach (SearchResult result in results)
		{
			TreeNode node = new SearchResultNode(document, result);
			base.Nodes.Add(node);
		}
	}
}
