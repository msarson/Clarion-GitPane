using ICSharpCode.SharpDevelop.DefaultEditor.Commands;
using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class GotoPrev : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new GotoPrevBookmark(PrevBookmark.AcceptOnlyStandardBookmarks);
}
