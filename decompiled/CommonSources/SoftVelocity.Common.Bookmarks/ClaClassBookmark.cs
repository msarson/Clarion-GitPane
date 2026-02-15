using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.ClassBrowser;

namespace SoftVelocity.Common.Bookmarks;

public class ClaClassBookmark : Bookmark
{
	private IClass @class;

	private bool showMenu;

	private string contextMenuPath;

	public bool ShowMenu => showMenu;

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

	public ClaClassBookmark(IDocument document, IClass @class, bool showMenu)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		DomRegion region = @class.Region;
		((Bookmark)this)._002Ector(document, Math.Max(((DomRegion)(ref region)).BeginLine - 1, 0));
		this.showMenu = showMenu;
		this.@class = @class;
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
		int sortOrder = 0;
		int iconIndexForClass = ClaClassNode.GetIconIndexForClass(@class, ref sortOrder);
		g.DrawImageUnscaled(ClassBrowserIconService.ImageList.Images[iconIndexForClass], p);
	}
}
