using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Common.CodeCompletion;

internal class GotoPopupMenuRenderer : ToolStripProfessionalRenderer
{
	public GotoPopupMenuRenderer()
	{
	}

	public GotoPopupMenuRenderer(ProfessionalColorTable colorTable)
		: base(colorTable)
	{
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		if (((ToolStripMenuItem)e.Item).ShortcutKeyDisplayString == e.Text)
		{
			e.TextColor = SystemColors.GrayText;
			base.OnRenderItemText(e);
		}
		else
		{
			base.OnRenderItemText(e);
		}
	}
}
