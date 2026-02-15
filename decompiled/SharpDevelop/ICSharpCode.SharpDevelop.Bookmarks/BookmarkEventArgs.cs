using System;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class BookmarkEventArgs : EventArgs
{
	private SDBookmark bookmark;

	public SDBookmark Bookmark => bookmark;

	public BookmarkEventArgs(SDBookmark bookmark)
	{
		this.bookmark = bookmark;
	}
}
