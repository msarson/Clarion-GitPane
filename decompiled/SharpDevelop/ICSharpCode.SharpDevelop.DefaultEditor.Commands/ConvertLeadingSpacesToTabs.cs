using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ConvertLeadingSpacesToTabs : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new ICSharpCode.TextEditor.Actions.ConvertLeadingSpacesToTabs();
}
