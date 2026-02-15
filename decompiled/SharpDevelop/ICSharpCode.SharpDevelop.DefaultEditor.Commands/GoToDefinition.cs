using ICSharpCode.SharpDevelop.DefaultEditor.Actions;
using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class GoToDefinition : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new ICSharpCode.SharpDevelop.DefaultEditor.Actions.GoToDefinition();
}
