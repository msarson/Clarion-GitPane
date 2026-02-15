using System;
using System.Drawing;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Debugging;

public class CurrentLineBookmark : SDMarkerBookmark
{
	private static CurrentLineBookmark instance;

	private static int startLine;

	private static int startColumn;

	private static int endLine;

	private static int endColumn;

	public override bool CanToggle => false;

	public static void SetPosition(IViewContent viewContent, int makerStartLine, int makerStartColumn, int makerEndLine, int makerEndColumn)
	{
		if (viewContent is ITextEditorControlProvider textEditorControlProvider)
		{
			SetPosition(textEditorControlProvider.TextEditorControl.FileName, textEditorControlProvider.TextEditorControl.Document, makerStartLine, makerStartColumn, makerEndLine, makerEndColumn);
		}
		else
		{
			Remove();
		}
	}

	public static void SetPosition(string fileName, IDocument document, int makerStartLine, int makerStartColumn, int makerEndLine, int makerEndColumn)
	{
		Remove();
		startLine = makerStartLine;
		startColumn = makerStartColumn;
		endLine = makerEndLine;
		endColumn = makerEndColumn;
		document.GetLineSegment(startLine - 1);
		instance = new CurrentLineBookmark(fileName, document, startLine - 1);
		document.BookmarkManager.AddMark(instance);
		document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, startLine - 1, endLine - 1));
		document.CommitUpdate();
	}

	public static void Remove()
	{
		if (instance != null)
		{
			instance.Document.BookmarkManager.RemoveMark(instance);
			instance.RemoveMarker();
			instance = null;
		}
	}

	public CurrentLineBookmark(string fileName, IDocument document, int startLine)
		: base(fileName, document, startLine)
	{
		base.IsSaved = false;
		base.IsVisibleInBookmarkPad = false;
	}

	public override void Draw(IconBarMargin margin, Graphics g, Point p)
	{
		margin.DrawArrow(g, p.Y);
	}

	protected override TextMarker CreateMarker()
	{
		LineSegment lineSegment = base.Document.GetLineSegment(startLine - 1);
		TextMarker textMarker = new TextMarker(lineSegment.Offset + startColumn - 1, Math.Max(endColumn - startColumn, 1), TextMarkerType.SolidBlock, Color.Yellow, Color.Blue);
		base.Document.MarkerStrategy.InsertMarker(0, textMarker);
		return textMarker;
	}
}
