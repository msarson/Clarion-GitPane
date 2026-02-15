using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class ToUpperCase : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new ICSharpCode.TextEditor.Actions.ToUpperCase();
}
