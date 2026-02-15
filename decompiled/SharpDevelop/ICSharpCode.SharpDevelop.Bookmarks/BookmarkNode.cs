using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class BookmarkNode : ExtTreeNode
{
	private SDBookmark bookmark;

	private SizeF spaceSize;

	private static StringFormat sf = (StringFormat)StringFormat.GenericTypographic.Clone();

	private LineSegment line;

	private string positionText;

	public SDBookmark Bookmark => bookmark;

	public BookmarkNode(SDBookmark bookmark)
	{
		drawDefault = false;
		this.bookmark = bookmark;
		base.Tag = bookmark;
		base.Checked = bookmark.IsEnabled;
		positionText = "(" + (bookmark.LineNumber + 1) + ") ";
		bookmark.DocumentChanged += BookmarkDocumentChanged;
		bookmark.LineNumberChanged += BookmarkLineNumberChanged;
		if (bookmark.Document != null)
		{
			BookmarkDocumentChanged(null, null);
		}
		else
		{
			base.Text = positionText;
		}
	}

	public override void CheckedChanged()
	{
		bookmark.IsEnabled = base.Checked;
	}

	private void BookmarkDocumentChanged(object sender, EventArgs e)
	{
		if (bookmark.Document != null)
		{
			line = bookmark.Document.GetLineSegment(Math.Min(bookmark.LineNumber, bookmark.Document.TotalNumberOfLines));
			base.Text = positionText + bookmark.Document.GetText(line);
		}
	}

	private void BookmarkLineNumberChanged(object sender, EventArgs e)
	{
		positionText = "(" + (bookmark.LineNumber + 1) + ") ";
		BookmarkDocumentChanged(sender, e);
	}

	protected override int MeasureItemWidth(DrawTreeNodeEventArgs e)
	{
		Graphics graphics = e.Graphics;
		int num = MeasureTextWidth(graphics, positionText, ExtTreeNode.BoldMonospacedFont);
		if (line != null && !line.IsDeleted)
		{
			num += MeasureTextWidth(graphics, bookmark.Document.GetText(line).Replace("\t", "    "), ExtTreeNode.BoldMonospacedFont);
		}
		return num;
	}

	protected override void DrawForeground(DrawTreeNodeEventArgs e, float x)
	{
		Graphics graphics = e.Graphics;
		DrawText(e, positionText, SystemBrushes.WindowText, ExtTreeNode.RegularBigFont, ref x);
		spaceSize = graphics.MeasureString("-", ExtTreeNode.RegularBigFont, new PointF(0f, 0f), StringFormat.GenericTypographic);
		if (line != null && !line.IsDeleted)
		{
			DrawLine(graphics, line, e.Bounds.Y, x, e.State);
		}
	}

	public override void ActivateItem()
	{
		FileService.JumpToFilePosition(bookmark.FileName, bookmark.LineNumber, 0);
	}

	private float DrawDocumentWord(Graphics g, string word, PointF position, Font font, Color foreColor)
	{
		if (word == null || word.Length == 0)
		{
			return 0f;
		}
		SizeF sizeF = g.MeasureString(word, font, 32768, sf);
		g.DrawString(word, font, BrushRegistry.GetBrush(foreColor), position, sf);
		return sizeF.Width;
	}

	private void DrawLine(Graphics g, LineSegment line, float yPos, float xPos, TreeNodeStates state)
	{
		int num = 0;
		if (line.Words != null)
		{
			foreach (TextWord word in line.Words)
			{
				switch (word.Type)
				{
				case TextWordType.Space:
					xPos += spaceSize.Width;
					num++;
					break;
				case TextWordType.Tab:
					xPos += spaceSize.Width * 4f;
					num++;
					break;
				case TextWordType.Word:
					xPos += DrawDocumentWord(g, word.Word, new PointF(xPos, yPos), word.Bold ? ExtTreeNode.BoldMonospacedFont : ExtTreeNode.RegularMonospacedFont, GetTextColor(state, word.Color));
					num += word.Word.Length;
					break;
				}
			}
			return;
		}
		DrawDocumentWord(g, bookmark.Document.GetText(line), new PointF(xPos, yPos), ExtTreeNode.RegularMonospacedFont, GetTextColor(state, Color.Black));
	}
}
