using ICSharpCode.SharpDevelop.DefaultEditor.Commands;
using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class ClearBookmarks : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new ClearAllBookmarks(PrevBookmark.AcceptOnlyStandardBookmarks);
}
