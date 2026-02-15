using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common.Bookmarks;

public abstract class ClaMemberBookmark : Bookmark
{
	private IMember member;

	private bool showMenu;

	private string contextMenuPath;

	public bool ShowMenu => showMenu;

	public IMember Member => member;

	public string ContextMenuPath
	{
		get
		{
			return contextMenuPath;
		}
		set
		{
			contextMenuPath = value;
		}
	}

	public abstract int IconIndex { get; }

	public ClaMemberBookmark(IDocument document, IMember member, bool showMenu)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		DomRegion region = member.Region;
		((Bookmark)this)._002Ector(document, ((DomRegion)(ref region)).BeginLine - 1);
		this.showMenu = showMenu;
		this.member = member;
	}

	public ClaMemberBookmark(IDocument document, IMember member, int line, bool showMenu)
		: base(document, line - 1)
	{
		this.showMenu = showMenu;
		this.member = member;
	}

	public override bool Click(Control parent, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && showMenu)
		{
			MenuService.ShowContextMenu((object)this, ContextMenuPath, parent, e.X, e.Y);
			return true;
		}
		return false;
	}

	public override void Draw(IconBarMargin margin, Graphics g, Point p)
	{
		g.DrawImageUnscaled(ClassBrowserIconService.ImageList.Images[IconIndex], p);
	}
}
