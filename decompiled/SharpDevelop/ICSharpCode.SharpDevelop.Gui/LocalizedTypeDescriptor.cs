using System;
using System.Collections;
using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.Gui;

public class LocalizedTypeDescriptor : ICustomTypeDescriptor
{
	private string defaultProperty;

	private ArrayList properties = new ArrayList();

	public ArrayList Properties => properties;

	public string DefaultProperty
	{
		get
		{
			return defaultProperty;
		}
		set
		{
			defaultProperty = value;
		}
	}

	public object GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}

	public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		return new PropertyDescriptorCollection((PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor)));
	}

	public PropertyDescriptorCollection GetProperties()
	{
		return GetProperties(null);
	}

	public EventDescriptorCollection GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(this, attributes, noCustomTypeDesc: true);
	}

	public EventDescriptorCollection GetEvents()
	{
		return TypeDescriptor.GetEvents(this, noCustomTypeDesc: true);
	}

	public object GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(this, editorBaseType, noCustomTypeDesc: true);
	}

	public PropertyDescriptor GetDefaultProperty()
	{
		return null;
	}

	public EventDescriptor GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(this, noCustomTypeDesc: true);
	}

	public TypeConverter GetConverter()
	{
		return TypeDescriptor.GetConverter(this, noCustomTypeDesc: true);
	}

	public string GetComponentName()
	{
		return TypeDescriptor.GetComponentName(this, noCustomTypeDesc: true);
	}

	public string GetClassName()
	{
		return TypeDescriptor.GetClassName(this, noCustomTypeDesc: true);
	}

	public AttributeCollection GetAttributes()
	{
		return TypeDescriptor.GetAttributes(this, noCustomTypeDesc: true);
	}
}
