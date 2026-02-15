using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class SearchResultNode : ExtTreeNode
{
	private SearchResult result;

	private TextLocation startPosition;

	private string positionText;

	private string displayText;

	private string specialText;

	private bool showFileName;

	private DrawableLine drawableLine;

	public bool ShowFileName
	{
		get
		{
			return showFileName;
		}
		set
		{
			showFileName = value;
			if (showFileName)
			{
				base.Text = displayText + FileNameText;
			}
			else
			{
				base.Text = displayText;
			}
		}
	}

	private string FileNameText => StringParser.Parse(" ${res:MainWindow.Windows.SearchResultPanel.In} ") + Path.GetFileName(result.FileName) + "(" + Path.GetDirectoryName(result.FileName) + ")";

	public SearchResultNode(IDocument document, SearchResult result)
	{
		drawDefault = false;
		this.result = result;
		startPosition = result.GetStartPosition(document);
		TextLocation endPosition = result.GetEndPosition(document);
		positionText = "(" + (startPosition.Y + 1) + ", " + (startPosition.X + 1) + ") ";
		LineSegment lineSegment = document.GetLineSegment(startPosition.Y);
		drawableLine = new DrawableLine(document, lineSegment, ExtTreeNode.RegularMonospacedFont, ExtTreeNode.BoldMonospacedFont);
		drawableLine.SetBold(0, drawableLine.LineLength, bold: false);
		if (startPosition.Y == endPosition.Y)
		{
			drawableLine.SetBold(startPosition.X, endPosition.X, bold: true);
		}
		specialText = result.DisplayText;
		if (specialText != null)
		{
			displayText = positionText + specialText;
		}
		else
		{
			displayText = positionText + document.GetText(lineSegment).Replace("\t", "    ");
		}
		base.Text = displayText;
	}

	protected override int MeasureItemWidth(DrawTreeNodeEventArgs e)
	{
		Graphics graphics = e.Graphics;
		int num = MeasureTextWidth(graphics, displayText, ExtTreeNode.BoldMonospacedFont);
		if (ShowFileName)
		{
			float num2 = drawableLine.GetSpaceSize(graphics).Width * 6f;
			num = (int)((float)(int)(((float)(num + 2) + num2) / num2) * num2);
			num += MeasureTextWidth(graphics, FileNameText, ExtTreeNode.ItalicBigFont);
		}
		return num;
	}

	protected override void DrawForeground(DrawTreeNodeEventArgs e, float x)
	{
		Graphics graphics = e.Graphics;
		DrawText(e, positionText, SystemBrushes.WindowText, ExtTreeNode.RegularBigFont, ref x);
		if (specialText != null)
		{
			DrawText(e, specialText, SystemBrushes.WindowText, ExtTreeNode.RegularBigFont, ref x);
		}
		else
		{
			x -= (float)e.Bounds.X;
			drawableLine.DrawLine(graphics, ref x, e.Bounds.X, e.Bounds.Y, GetTextColor(e.State, Color.Empty));
		}
		if (ShowFileName)
		{
			float num = drawableLine.GetSpaceSize(graphics).Width * 6f;
			x = (int)((float)(int)((x + 2f + num) / num) * num);
			x += (float)e.Bounds.X;
			DrawText(e, FileNameText, SystemBrushes.GrayText, ExtTreeNode.ItalicBigFont, ref x);
		}
	}

	public override void ActivateItem()
	{
		FileService.JumpToFilePosition(result.FileName, startPosition.Y, startPosition.X);
	}
}
