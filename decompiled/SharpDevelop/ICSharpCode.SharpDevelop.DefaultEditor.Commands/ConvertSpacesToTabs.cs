using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ConvertSpacesToTabs : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new ICSharpCode.TextEditor.Actions.ConvertSpacesToTabs();
}
