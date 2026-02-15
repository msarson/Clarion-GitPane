using System;
using System.Drawing;
using System.IO;

namespace ICSharpCode.SharpDevelop;

public class TextNavigationPoint : DefaultNavigationPoint
{
	private const int THREASHOLD = 5;

	private string content;

	public int LineNumber => ((Point)base.NavigationData).Y;

	public int Column => ((Point)base.NavigationData).X;

	public override string Description => $"{LineNumber}: {content}";

	public override string FullDescription => $"{Path.GetFileName(FileName)} - {Description}";

	public TextNavigationPoint()
		: this(string.Empty, 0, 0)
	{
	}

	public TextNavigationPoint(string fileName)
		: this(fileName, 0, 0)
	{
	}

	public TextNavigationPoint(string fileName, int lineNumber, int column)
		: this(fileName, lineNumber, column, string.Empty)
	{
	}

	public TextNavigationPoint(string fileName, int lineNumber, int column, string content)
		: base(fileName, new Point(column, lineNumber))
	{
		if (string.IsNullOrEmpty(content))
		{
			this.content = string.Empty;
		}
		else
		{
			this.content = content.Trim();
		}
	}

	public override void JumpTo()
	{
		FileService.JumpToFilePosition(FileName, LineNumber, Column);
	}

	public override void ContentChanging(object sender, EventArgs e)
	{
	}

	public override int CompareTo(object obj)
	{
		int num = base.CompareTo(obj);
		if (num != 0)
		{
			return num;
		}
		TextNavigationPoint textNavigationPoint = obj as TextNavigationPoint;
		if (LineNumber == textNavigationPoint.LineNumber)
		{
			return 0;
		}
		if (LineNumber > textNavigationPoint.LineNumber)
		{
			return 1;
		}
		return -1;
	}

	public override bool Equals(object obj)
	{
		TextNavigationPoint textNavigationPoint = obj as TextNavigationPoint;
		if (textNavigationPoint == null)
		{
			return false;
		}
		if (FileName.Equals(textNavigationPoint.FileName))
		{
			return Math.Abs(LineNumber - textNavigationPoint.LineNumber) <= 5;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return FileName.GetHashCode() ^ LineNumber.GetHashCode();
	}
}
