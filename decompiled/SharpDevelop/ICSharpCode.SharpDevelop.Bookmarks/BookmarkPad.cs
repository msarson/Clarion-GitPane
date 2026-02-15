using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public sealed class BookmarkPad : BookmarkPadBase
{
	private static BookmarkPad instance;

	public static BookmarkPad Instance
	{
		get
		{
			if (instance == null)
			{
				WorkbenchSingleton.Workbench.GetPad(typeof(BookmarkPad)).CreatePad();
			}
			return instance;
		}
	}

	public BookmarkPad()
	{
		instance = this;
	}
}
