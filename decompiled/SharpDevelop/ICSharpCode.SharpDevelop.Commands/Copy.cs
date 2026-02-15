using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class Copy : AbstractClipboardCommand
{
	protected override bool GetEnabled(IClipboardHandler editable)
	{
		return editable.EnableCopy;
	}

	protected override void Run(IClipboardHandler editable)
	{
		editable.Copy();
	}
}
