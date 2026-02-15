using System;
using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.Gui;

public class LocalizedObject : ICustomTypeDescriptor
{
	private PropertyDescriptorCollection globalizedProps;

	string ICustomTypeDescriptor.GetClassName()
	{
		return TypeDescriptor.GetClassName(this, noCustomTypeDesc: true);
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return TypeDescriptor.GetAttributes(this, noCustomTypeDesc: true);
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return TypeDescriptor.GetComponentName(this, noCustomTypeDesc: true);
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return TypeDescriptor.GetConverter(this, noCustomTypeDesc: true);
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(this, noCustomTypeDesc: true);
	}

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(this, noCustomTypeDesc: true);
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(this, editorBaseType, noCustomTypeDesc: true);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(this, attributes, noCustomTypeDesc: true);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return TypeDescriptor.GetEvents(this, noCustomTypeDesc: true);
	}

	protected virtual void FilterProperties(PropertyDescriptorCollection globalizedProps)
	{
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		if (globalizedProps == null)
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this, attributes, noCustomTypeDesc: true);
			globalizedProps = new PropertyDescriptorCollection(null);
			foreach (PropertyDescriptor item in properties)
			{
				globalizedProps.Add(new LocalizedPropertyDescriptor(item));
			}
			FilterProperties(globalizedProps);
		}
		return globalizedProps;
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		if (globalizedProps == null)
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this, noCustomTypeDesc: true);
			globalizedProps = new PropertyDescriptorCollection(null);
			foreach (PropertyDescriptor item in properties)
			{
				globalizedProps.Add(new LocalizedPropertyDescriptor(item));
			}
			FilterProperties(globalizedProps);
		}
		return globalizedProps;
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}

	public virtual void InformSetValue(LocalizedPropertyDescriptor localizedPropertyDescriptor, object component, object value)
	{
	}
}
