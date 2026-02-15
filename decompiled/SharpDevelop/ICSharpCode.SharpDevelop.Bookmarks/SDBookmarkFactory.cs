using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class SDBookmarkFactory : IBookmarkFactory
{
	private string fileName;

	private ICSharpCode.TextEditor.Document.BookmarkManager manager;

	public SDBookmarkFactory(ICSharpCode.TextEditor.Document.BookmarkManager manager)
	{
		this.manager = manager;
	}

	public virtual void ChangeFilename(string newFileName)
	{
		fileName = newFileName;
		foreach (Bookmark mark in manager.Marks)
		{
			if (mark is SDBookmark sDBookmark)
			{
				sDBookmark.FileName = newFileName;
			}
		}
	}

	public virtual Bookmark CreateBookmark(IDocument document, int lineNumber)
	{
		return new SDBookmark(fileName, document, lineNumber);
	}
}
