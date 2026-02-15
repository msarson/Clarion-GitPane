using System;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public interface IPropertyValueCreator
{
	bool CanCreateValueForType(Type propertyType);

	object CreateValue(Type propertyType, string valueString);
}
