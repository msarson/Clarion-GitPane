using System;
using System.ComponentModel;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class LocalizedProperty : PropertyDescriptor
{
	private string category;

	private string description;

	private string name;

	private string type;

	private string localizedName;

	private TypeConverter typeConverterObject;

	private object defaultValue;

	public TypeConverter TypeConverterObject
	{
		get
		{
			return typeConverterObject;
		}
		set
		{
			typeConverterObject = value;
		}
	}

	public object DefaultValue
	{
		get
		{
			return defaultValue;
		}
		set
		{
			defaultValue = value;
		}
	}

	public string LocalizedName
	{
		get
		{
			if (localizedName == null)
			{
				return null;
			}
			return StringParser.Parse(localizedName);
		}
		set
		{
			localizedName = value;
		}
	}

	public override bool IsReadOnly => false;

	public override string DisplayName
	{
		get
		{
			if (localizedName != null && localizedName.Length > 0)
			{
				return LocalizedName;
			}
			return Name;
		}
	}

	public override string Category => StringParser.Parse(category);

	public override string Description => StringParser.Parse(description);

	public override Type PropertyType => Type.GetType(type);

	public override Type ComponentType => Type.GetType(type);

	public override TypeConverter Converter
	{
		get
		{
			if (typeConverterObject != null)
			{
				return typeConverterObject;
			}
			return base.Converter;
		}
	}

	public override object GetValue(object component)
	{
		string text = StringParser.Properties["Properties." + Name];
		if (typeConverterObject is BooleanTypeConverter)
		{
			return bool.Parse(text);
		}
		return text;
	}

	public override void SetValue(object component, object val)
	{
		if (typeConverterObject != null)
		{
			StringParser.Properties["Properties." + Name] = typeConverterObject.ConvertFrom(val).ToString();
		}
		else
		{
			StringParser.Properties["Properties." + Name] = val.ToString();
		}
	}

	public override bool ShouldSerializeValue(object component)
	{
		return false;
	}

	public override bool CanResetValue(object component)
	{
		return defaultValue != null;
	}

	public override void ResetValue(object component)
	{
		SetValue(component, defaultValue);
	}

	public LocalizedProperty(string name, string type, string category, string description)
		: base(name, null)
	{
		this.category = category;
		this.description = description;
		this.name = name;
		this.type = type;
	}
}
