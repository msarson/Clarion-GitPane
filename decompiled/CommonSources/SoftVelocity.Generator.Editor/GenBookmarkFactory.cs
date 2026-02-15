using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Generator.Editor;

public class GenBookmarkFactory : SDBookmarkFactory
{
	private string fileName = string.Empty;

	private BookmarkManager manager;

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				fileName = string.Empty;
				manager.Clear();
			}
			else
			{
				fileName = value;
			}
		}
	}

	public GenBookmarkFactory(BookmarkManager manager)
		: base(manager)
	{
		this.manager = manager;
	}

	public override void ChangeFilename(string newFileName)
	{
	}

	public override Bookmark CreateBookmark(IDocument document, int lineNumber)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		SDBookmark val = new SDBookmark(FileName, document, lineNumber);
		val.IsSaved = false;
		return (Bookmark)(object)val;
	}
}
