using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.ClarionNet.ReportItems;
using VisualHint.SmartPropertyGrid;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public class CopyControl : AbstractClarionReportCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (base.View.BaseReportDesignerControl != null && base.View.BaseReportDesignerControl.SelectedObject is ReportItem && PropertyPad.Grid != null && ((PropertyGrid)(object)PropertyPad.Grid).SelectedObject != base.View.ReportDesignerControl.ReportSettings)
			{
				return true;
			}
			return false;
		}
	}

	public override void Run()
	{
		base.View.ReportDesignerControl.CopyControl();
	}
}
