using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class GotoLineNumber : AbstractMenuCommand
{
	public override void Run()
	{
		GotoDialog.ShowSingleInstance();
	}
}
