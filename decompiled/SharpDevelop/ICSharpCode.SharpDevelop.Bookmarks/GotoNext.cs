using ICSharpCode.SharpDevelop.DefaultEditor.Commands;
using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class GotoNext : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new GotoNextBookmark(PrevBookmark.AcceptOnlyStandardBookmarks);
}
