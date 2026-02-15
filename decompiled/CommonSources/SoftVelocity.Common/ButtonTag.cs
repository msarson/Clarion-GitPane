using System.Windows.Forms;

namespace SoftVelocity.Common;

internal struct ButtonTag
{
	public Control entryControl;

	public string title;

	public string filter;

	public ButtonTag(string f, string t, Control c)
	{
		entryControl = c;
		title = t;
		filter = f;
	}
}
