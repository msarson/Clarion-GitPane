using System;
using System.ComponentModel;
using System.Globalization;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class BooleanTypeConverter : TypeConverter
{
	private string True => StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Components.BooleanTypeConverter.TrueString}");

	private string False => StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Components.BooleanTypeConverter.FalseString}");

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (!(sourceType == typeof(bool)))
		{
			return sourceType == typeof(string);
		}
		return true;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (!(destinationType == typeof(bool)))
		{
			return destinationType == typeof(string);
		}
		return true;
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		return true;
	}

	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		return new StandardValuesCollection(new object[2] { True, False });
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			return value.ToString() == True;
		}
		return value;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value is bool)
		{
			if (!(bool)value)
			{
				return False;
			}
			return True;
		}
		return value;
	}
}
