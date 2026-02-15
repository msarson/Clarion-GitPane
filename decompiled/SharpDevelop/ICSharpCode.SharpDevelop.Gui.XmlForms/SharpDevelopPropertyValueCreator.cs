using System;
using System.Drawing;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public class SharpDevelopPropertyValueCreator : IPropertyValueCreator
{
	public bool CanCreateValueForType(Type propertyType)
	{
		if (!(propertyType == typeof(Icon)))
		{
			return propertyType == typeof(Image);
		}
		return true;
	}

	public object CreateValue(Type propertyType, string valueString)
	{
		if (propertyType == typeof(Icon))
		{
			return ResourceService.GetIcon(valueString);
		}
		if (propertyType == typeof(Image))
		{
			return ResourceService.GetBitmap(valueString);
		}
		return null;
	}
}
