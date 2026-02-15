using System;
using System.ComponentModel;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class LocalizedPropertyDescriptor : PropertyDescriptor
{
	private PropertyDescriptor basePropertyDescriptor;

	private string localizedName = string.Empty;

	private string localizedDescription = string.Empty;

	private string localizedCategory = string.Empty;

	private TypeConverter customTypeConverter;

	public override bool IsReadOnly => basePropertyDescriptor.IsReadOnly;

	public override string Name => basePropertyDescriptor.Name;

	public override Type PropertyType => basePropertyDescriptor.PropertyType;

	public override Type ComponentType => basePropertyDescriptor.ComponentType;

	public override string DisplayName => StringParser.Parse(localizedName);

	public override string Description => StringParser.Parse(localizedDescription);

	public override string Category => StringParser.Parse(localizedCategory);

	public override TypeConverter Converter
	{
		get
		{
			if (customTypeConverter != null)
			{
				return customTypeConverter;
			}
			return base.Converter;
		}
	}

	public LocalizedPropertyDescriptor(PropertyDescriptor basePropertyDescriptor)
		: base(basePropertyDescriptor)
	{
		LocalizedPropertyAttribute localizedPropertyAttribute = null;
		foreach (Attribute attribute in basePropertyDescriptor.Attributes)
		{
			localizedPropertyAttribute = attribute as LocalizedPropertyAttribute;
			if (localizedPropertyAttribute != null)
			{
				break;
			}
		}
		if (localizedPropertyAttribute != null)
		{
			localizedName = localizedPropertyAttribute.Name;
			localizedDescription = localizedPropertyAttribute.Description;
			localizedCategory = localizedPropertyAttribute.Category;
		}
		else
		{
			localizedName = basePropertyDescriptor.Name;
			localizedDescription = basePropertyDescriptor.Description;
			localizedCategory = basePropertyDescriptor.Category;
		}
		this.basePropertyDescriptor = basePropertyDescriptor;
		if (basePropertyDescriptor.PropertyType == typeof(bool))
		{
			customTypeConverter = new BooleanTypeConverter();
		}
	}

	public override bool CanResetValue(object component)
	{
		return basePropertyDescriptor.CanResetValue(component);
	}

	public override object GetValue(object component)
	{
		return basePropertyDescriptor.GetValue(component);
	}

	public override void ResetValue(object component)
	{
		basePropertyDescriptor.ResetValue(component);
		if (component is LocalizedObject)
		{
			((LocalizedObject)component).InformSetValue(this, component, null);
		}
	}

	public override bool ShouldSerializeValue(object component)
	{
		return basePropertyDescriptor.ShouldSerializeValue(component);
	}

	public override void SetValue(object component, object value)
	{
		if (customTypeConverter != null && value.GetType() != PropertyType)
		{
			basePropertyDescriptor.SetValue(component, customTypeConverter.ConvertFrom(value));
		}
		else
		{
			basePropertyDescriptor.SetValue(component, value);
		}
		if (component is LocalizedObject)
		{
			((LocalizedObject)component).InformSetValue(this, component, value);
		}
	}
}
