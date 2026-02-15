using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class DeleteMark : AbstractMenuCommand
{
	public override void Run()
	{
		BookmarkNode currentNode = ((BookmarkPadBase)Owner).CurrentNode;
		if (currentNode != null)
		{
			if (currentNode.Bookmark.Document != null)
			{
				currentNode.Bookmark.Document.BookmarkManager.RemoveMark(currentNode.Bookmark);
			}
			else
			{
				BookmarkManager.RemoveMark(currentNode.Bookmark);
			}
			WorkbenchSingleton.MainForm.Refresh();
		}
	}
}
