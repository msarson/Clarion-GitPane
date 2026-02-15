using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class PropertyGridPanel : AbstractOptionPanel
{
	private PropertyGrid grid = new PropertyGrid();

	public override bool ReceiveDialogMessage(DialogMessage message)
	{
		return true;
	}

	public PropertyGridPanel(string name, object customizer)
	{
		grid.SelectedObject = customizer;
		grid.Size = base.ClientSize;
		grid.Width -= 16;
		grid.Height -= 16;
		grid.Location = new Point(8, 8);
		grid.CommandsVisibleIfAvailable = true;
		grid.AutoScrollMinSize = new Size(0, 0);
		grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		grid.ToolbarVisible = false;
		base.Controls.Add(grid);
	}
}
