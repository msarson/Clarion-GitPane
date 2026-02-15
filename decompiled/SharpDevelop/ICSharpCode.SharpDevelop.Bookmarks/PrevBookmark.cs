using ICSharpCode.SharpDevelop.DefaultEditor.Commands;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class PrevBookmark : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new GotoPrevBookmark(AcceptOnlyStandardBookmarks);

	public static bool AcceptOnlyStandardBookmarks(Bookmark mark)
	{
		return mark is SDBookmark;
	}
}
