using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop.Gui;

public class CustomTypeConverter : TypeConverter
{
	private TemplateType templateType;

	public CustomTypeConverter(TemplateType templateType)
	{
		this.templateType = templateType;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(string);
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
		ArrayList arrayList = new ArrayList();
		foreach (DictionaryEntry pair in templateType.Pairs)
		{
			arrayList.Add(pair.Key);
		}
		return new StandardValuesCollection(arrayList);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (templateType.Pairs[value] != null)
		{
			return templateType.Pairs[value];
		}
		return value.ToString();
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		foreach (DictionaryEntry pair in templateType.Pairs)
		{
			if (pair.Value.ToString() == value.ToString())
			{
				return pair.Key;
			}
		}
		return value.ToString();
	}
}
