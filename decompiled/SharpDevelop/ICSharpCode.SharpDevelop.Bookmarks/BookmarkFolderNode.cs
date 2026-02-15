using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class BookmarkFolderNode : ExtFolderNode
{
	private List<SDBookmark> marks = new List<SDBookmark>();

	private string fileName;

	private string fileNameText;

	private string occurences;

	private Image icon;

	public List<SDBookmark> Marks => marks;

	public BookmarkFolderNode(string fileName)
	{
		drawDefault = false;
		this.fileName = fileName;
		fileNameText = Path.GetFileName(fileName) + StringParser.Parse(" ${res:MainWindow.Windows.SearchResultPanel.In} ") + Path.GetDirectoryName(fileName);
		icon = IconService.GetBitmap(IconService.GetImageForFile(fileName));
		base.Nodes.Add(new TreeNode());
	}

	public void SetText()
	{
		if (marks.Count == 1)
		{
			occurences = " (1 bookmark)";
		}
		else
		{
			occurences = " (" + marks.Count + " bookmarks)";
		}
		base.Text = fileNameText + occurences;
	}

	protected override int MeasureItemWidth(DrawTreeNodeEventArgs e)
	{
		Graphics graphics = e.Graphics;
		int num = MeasureTextWidth(graphics, fileNameText, ExtTreeNode.RegularBigFont);
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
		DrawText(e, fileNameText, SystemBrushes.WindowText, ExtTreeNode.RegularBigFont, ref x);
		DrawText(e, occurences, SystemBrushes.GrayText, ExtTreeNode.ItalicBigFont, ref x);
	}

	public void AddMark(SDBookmark mark)
	{
		int num = -1;
		for (int i = 0; i < marks.Count; i++)
		{
			if (mark.LineNumber < marks[i].LineNumber)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			marks.Add(mark);
		}
		else
		{
			marks.Insert(num, mark);
		}
		if (isInitialized)
		{
			BookmarkNode bookmarkNode = new BookmarkNode(mark);
			if (num < 0)
			{
				base.Nodes.Add(bookmarkNode);
			}
			else
			{
				base.Nodes.Insert(num, bookmarkNode);
			}
			bookmarkNode.EnsureVisible();
		}
		SetText();
	}

	public void RemoveMark(SDBookmark mark)
	{
		marks.Remove(mark);
		if (isInitialized)
		{
			for (int i = 0; i < base.Nodes.Count; i++)
			{
				if (((BookmarkNode)base.Nodes[i]).Bookmark == mark)
				{
					base.Nodes.RemoveAt(i);
					break;
				}
			}
		}
		SetText();
	}

	protected override void Initialize()
	{
		base.Nodes.Clear();
		if (marks.Count <= 0)
		{
			return;
		}
		IDocument document = marks[0].Document;
		if (document != null && document.HighlightingStrategy == null)
		{
			document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategyForFile(fileName);
		}
		foreach (SDBookmark mark in marks)
		{
			TreeNode node = new BookmarkNode(mark);
			base.Nodes.Add(node);
		}
	}
}
