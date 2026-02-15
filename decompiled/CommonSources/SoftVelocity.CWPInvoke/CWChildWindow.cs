using System.ComponentModel;
using System.Windows.Forms;

namespace SoftVelocity.CWPInvoke;

[DesignTimeVisible(true)]
[ToolboxItem(true)]
public class CWChildWindow : CWInternalDialogWindow
{
	public CWChildWindow(CWDialogForm f)
		: base(f)
	{
	}

	private void OnCloseDialogWindow(object sender)
	{
		base.ParentForm.Close();
	}

	public void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		if (Hosted == null)
		{
			e.Cancel = true;
			RequestClose();
		}
	}
}
