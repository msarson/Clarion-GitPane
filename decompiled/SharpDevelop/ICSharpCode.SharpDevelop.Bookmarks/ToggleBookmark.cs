using ICSharpCode.SharpDevelop.DefaultEditor.Commands;
using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class ToggleBookmark : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new ICSharpCode.TextEditor.Actions.ToggleBookmark();
}
