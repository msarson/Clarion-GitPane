using System.ComponentModel.Design;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class VertSpaceMakeEqual : AbstractWindowDesignerCommand
{
	public override CommandID CommandID => StandardCommands.VertSpaceMakeEqual;

	protected override bool CanExecuteCommand(IDesignerHost host)
	{
		ISelectionService selectionService = (ISelectionService)host.GetService(typeof(ISelectionService));
		return selectionService.SelectionCount > 1;
	}
}
