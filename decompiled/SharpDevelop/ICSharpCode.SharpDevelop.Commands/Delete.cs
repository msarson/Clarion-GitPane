using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class Delete : AbstractClipboardCommand
{
	protected override bool GetEnabled(IClipboardHandler editable)
	{
		return editable.EnableDelete;
	}

	protected override void Run(IClipboardHandler editable)
	{
		editable.Delete();
	}
}
