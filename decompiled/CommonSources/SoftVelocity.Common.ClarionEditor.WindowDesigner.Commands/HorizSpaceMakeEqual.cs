using System.ComponentModel.Design;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class HorizSpaceMakeEqual : AbstractWindowDesignerCommand
{
	public override CommandID CommandID => StandardCommands.HorizSpaceMakeEqual;

	protected override bool CanExecuteCommand(IDesignerHost host)
	{
		ISelectionService selectionService = (ISelectionService)host.GetService(typeof(ISelectionService));
		return selectionService.SelectionCount > 1;
	}
}
