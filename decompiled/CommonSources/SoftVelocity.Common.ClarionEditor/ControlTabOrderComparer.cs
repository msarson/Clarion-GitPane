using System.Collections.Generic;
using System.Windows.Forms;

namespace SoftVelocity.Common.ClarionEditor;

public class ControlTabOrderComparer : IComparer<Control>
{
	public int Compare(Control x, Control y)
	{
		return x.TabIndex - y.TabIndex;
	}
}
