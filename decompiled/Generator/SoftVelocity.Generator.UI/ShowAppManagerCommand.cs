using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.UI;

public class ShowAppManagerCommand : AbstractMenuCommand
{
	public override void Run()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(ApplicationBrowserPad));
		if (pad != null)
		{
			pad.BringPadToFront();
			return;
		}
		pad = new PadDescriptor(typeof(ApplicationBrowserPad), "Applications", "");
		WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(pad);
	}
}
