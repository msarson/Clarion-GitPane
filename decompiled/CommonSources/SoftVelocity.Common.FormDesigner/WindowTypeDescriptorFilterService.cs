using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ICSharpCode.FormsDesigner.Services;

namespace SoftVelocity.Common.FormDesigner;

public class WindowTypeDescriptorFilterService : TypeDescriptorFilterService, ITypeDescriptorFilterService
{
	public new bool FilterProperties(IComponent component, IDictionary properties)
	{
		base.FilterProperties(component, properties);
		PropertyDescriptor propertyDescriptor = (PropertyDescriptor)properties["DoubleBuffered"];
		if (propertyDescriptor != null)
		{
			properties["DoubleBuffered"] = TypeDescriptor.CreateProperty(typeof(Control), "DoubleBuffered", typeof(bool), BrowsableAttribute.No, DesignOnlyAttribute.No);
		}
		propertyDescriptor = (PropertyDescriptor)properties["AutoScaleMode"];
		if (propertyDescriptor != null)
		{
			properties["AutoScaleMode"] = TypeDescriptor.CreateProperty(typeof(DocumentDesigner), propertyDescriptor, DesignerSerializationVisibilityAttribute.Visible, BrowsableAttribute.No);
		}
		return false;
	}
}
