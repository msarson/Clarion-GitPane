using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public abstract class ClassMemberBookmark : Bookmark
{
	public const string ContextMenuPath = "/SharpDevelop/ViewContent/DefaultTextEditor/ClassMemberContextMenu";

	private IMember member;

	public IMember Member => member;

	public abstract int IconIndex { get; }

	public ClassMemberBookmark(IDocument document, IMember member)
		: base(document, GetLineNumberFromMember(document, member))
	{
		this.member = member;
	}

	private static int GetLineNumberFromMember(IDocument document, IMember member)
	{
		int num = member.Region.BeginLine - 1;
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
			MenuService.ShowContextMenu(this, "/SharpDevelop/ViewContent/DefaultTextEditor/ClassMemberContextMenu", parent, e.X, e.Y);
			return true;
		}
		return false;
	}

	public override void Draw(IconBarMargin margin, Graphics g, Point p)
	{
		g.DrawImageUnscaled(ClassBrowserIconService.ImageList.Images[IconIndex], p);
	}
}
