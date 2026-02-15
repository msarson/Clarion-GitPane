using System.Drawing;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Debugging;

public class BreakpointBookmark : SDMarkerBookmark
{
	private bool willBeHit = true;

	private static readonly Color defaultColor = Color.FromArgb(180, 38, 38);

	public virtual bool WillBeHit
	{
		get
		{
			return willBeHit;
		}
		set
		{
			willBeHit = value;
			if (base.Document != null && !base.Line.IsDeleted)
			{
				base.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, base.LineNumber));
				base.Document.CommitUpdate();
			}
		}
	}

	public BreakpointBookmark(string fileName, IDocument document, int lineNumber)
		: base(fileName, document, lineNumber)
	{
	}

	public override void Draw(IconBarMargin margin, Graphics g, Point p)
	{
		margin.DrawBreakpoint(g, p.Y, base.IsEnabled, WillBeHit);
	}

	protected override TextMarker CreateMarker()
	{
		if (base.LineNumber >= base.Document.TotalNumberOfLines)
		{
			base.LineNumber = base.Document.TotalNumberOfLines - 1;
		}
		LineSegment lineSegment = base.Document.GetLineSegment(base.LineNumber);
		TextMarker textMarker = new TextMarker(lineSegment.Offset, lineSegment.Length, TextMarkerType.SolidBlock, defaultColor, Color.White);
		base.Document.MarkerStrategy.AddMarker(textMarker);
		return textMarker;
	}
}
