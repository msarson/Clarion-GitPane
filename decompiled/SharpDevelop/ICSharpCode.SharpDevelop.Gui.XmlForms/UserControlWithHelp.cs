using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public class UserControlWithHelp : UserControl
{
	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.F1)
		{
			return FormWithHelp.DoF1(GetType());
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}
}
