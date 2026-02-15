using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public static class BookmarkManager
{
	private static List<SDBookmark> bookmarks = new List<SDBookmark>();

	public static List<SDBookmark> Bookmarks => bookmarks;

	public static event BookmarkEventHandler Removed;

	public static event BookmarkEventHandler Added;

	public static List<SDBookmark> GetBookmarks(string fileName)
	{
		List<SDBookmark> list = new List<SDBookmark>();
		foreach (SDBookmark bookmark in bookmarks)
		{
			if (bookmark.FileName != null && FileUtility.IsEqualFileName(bookmark.FileName, fileName))
			{
				list.Add(bookmark);
			}
		}
		return list;
	}

	public static void AddMark(SDBookmark bookmark)
	{
		if (!bookmarks.Contains(bookmark))
		{
			bookmarks.Add(bookmark);
			OnAdded(new BookmarkEventArgs(bookmark));
		}
	}

	public static void RemoveMark(SDBookmark bookmark)
	{
		bookmarks.Remove(bookmark);
		OnRemoved(new BookmarkEventArgs(bookmark));
	}

	public static void Clear()
	{
		while (bookmarks.Count > 0)
		{
			SDBookmark bookmark = bookmarks[bookmarks.Count - 1];
			bookmarks.RemoveAt(bookmarks.Count - 1);
			OnRemoved(new BookmarkEventArgs(bookmark));
		}
	}

	internal static void Initialize()
	{
		ProjectService.SolutionClosing += delegate
		{
			Clear();
		};
	}

	private static void OnRemoved(BookmarkEventArgs e)
	{
		if (BookmarkManager.Removed != null)
		{
			BookmarkManager.Removed(null, e);
		}
	}

	private static void OnAdded(BookmarkEventArgs e)
	{
		if (BookmarkManager.Added != null)
		{
			BookmarkManager.Added(null, e);
		}
	}

	public static List<SDBookmark> GetProjectBookmarks(IProject project)
	{
		List<SDBookmark> list = new List<SDBookmark>();
		foreach (SDBookmark bookmark in bookmarks)
		{
			if (bookmark.IsSaved && bookmark.FileName != null && project.IsFileInProject(bookmark.FileName))
			{
				list.Add(bookmark);
			}
		}
		return list;
	}
}
