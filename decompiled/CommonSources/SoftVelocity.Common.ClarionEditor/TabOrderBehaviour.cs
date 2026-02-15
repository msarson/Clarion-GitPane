using System.ComponentModel;
using System.Windows.Forms;

namespace SoftVelocity.Common.ClarionEditor;

public abstract class TabOrderBehaviour
{
	public virtual Control[] GetControlChildren(Control control)
	{
		Control[] array = new Control[control.Controls.Count];
		for (int i = 0; i < control.Controls.Count; i++)
		{
			array[i] = control.Controls[i];
		}
		return array;
	}

	public virtual Control GetControlParent(Control control)
	{
		return control.Parent;
	}

	public virtual void SetTabIndex(Control control, PropertyDescriptor tabIndexpropertyDescriptor, int tabIndex)
	{
		tabIndexpropertyDescriptor.SetValue(control, tabIndex);
	}
}
