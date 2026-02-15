using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using SoftVelocity.ClarionNet.WindowDesigner;

namespace SoftVelocity.Common.ClarionEditor;

public class WindowTabOrderBehaviour : TabOrderBehaviour
{
	private GeneralDesiner generalDesigner;

	public WindowTabOrderBehaviour(GeneralDesiner generalDesigner)
	{
		if (generalDesigner == null)
		{
			throw new ArgumentNullException("generalDesigner");
		}
		this.generalDesigner = generalDesigner;
	}

	public override Control[] GetControlChildren(Control control)
	{
		return generalDesigner.GetChildControls(control);
	}

	public override Control GetControlParent(Control control)
	{
		return generalDesigner.GetControlParent(control);
	}

	public override void SetTabIndex(Control control, PropertyDescriptor tabIndexpropertyDescriptor, int tabIndex)
	{
		Control controlParent = GetControlParent(control);
		ISite site = control.Site;
		IComponentChangeService componentChangeService = null;
		try
		{
			if (site != null)
			{
				componentChangeService = (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
				componentChangeService?.OnComponentChanging(control, tabIndexpropertyDescriptor);
			}
			generalDesigner.SetChildIndex(controlParent, control, tabIndex);
			generalDesigner.ResetTabOrder(generalDesigner);
		}
		finally
		{
			componentChangeService?.OnComponentChanged(control, tabIndexpropertyDescriptor, null, null);
		}
	}
}
