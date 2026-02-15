using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class IndentSelection : AbstractEditActionMenuCommand
{
	public override IEditAction EditAction => new FormatBuffer();
}
