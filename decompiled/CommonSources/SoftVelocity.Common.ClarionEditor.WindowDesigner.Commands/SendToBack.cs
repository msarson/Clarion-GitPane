using System.ComponentModel.Design;
using SoftVelocity.ClarionNet.WindowDesigner;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class SendToBack : AbstractWindowDesignerCommand
{
	public override CommandID CommandID => StandardCommands.SendToBack;

	public override void Run()
	{
		if (base.WindowDesignerView != null && !base.WindowDesignerView.IsReportDesigner && base.WindowDesignerView.WindowDesignerControl != null && GeneralDesiner.C6COMPATIBLE_MODE)
		{
			base.WindowDesignerView.WindowDesignerControl.SendToBackC6Controls();
		}
		else
		{
			base.Run();
		}
	}
}
