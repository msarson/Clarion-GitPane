using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class ClassBookmark : Bookmark
{
	public const string ContextMenuPath = "/SharpDevelop/ViewContent/DefaultTextEditor/ClassBookmarkContextMenu";

	private IClass @class;

	public IClass Class
	{
		get
		{
			return @class;
		}
		set
		{
			@class = value;
		}
	}

	public ClassBookmark(IDocument document, IClass @class)
		: base(document, GetLineNumberFromClass(document, @class))
	{
		this.@class = @class;
	}

	private static int GetLineNumberFromClass(IDocument document, IClass @class)
	{
		int num = @class.Region.BeginLine - 1;
		if (num < 0)
		{
			return 0;
		}
		if (document != null && num >= document.TotalNumberOfLines)
		{
			return document.TotalNumberOfLines - 1;
		}
		return num;
	}

	public override bool Click(Control parent, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			MenuService.ShowContextMenu(this, "/SharpDevelop/ViewContent/DefaultTextEditor/ClassBookmarkContextMenu", parent, e.X, e.Y);
			return true;
		}
		return false;
	}

	public override void Draw(IconBarMargin margin, Graphics g, Point p)
	{
		g.DrawImageUnscaled(ClassBrowserIconService.ImageList.Images[ClassBrowserIconService.GetIcon(@class)], p);
	}
}
