using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class RemoveLeadingWS : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new ICSharpCode.TextEditor.Actions.RemoveLeadingWS();
}
