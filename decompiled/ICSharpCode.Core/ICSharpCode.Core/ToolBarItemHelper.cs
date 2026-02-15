using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public static class ToolBarItemHelper
{
	public static bool IsCodonContainsWidth(Codon codon)
	{
		return codon.Properties["width"] != null;
	}

	public static void SetControlWidth(Codon codon, ToolStripControlHost control)
	{
		if (codon.Properties.Contains("width"))
		{
			int result = 0;
			if (int.TryParse(codon.Properties["width"].Trim('"'), out result) && result != control.Size.Width)
			{
				control.Size = new Size(result, control.Size.Height);
			}
		}
	}
}
