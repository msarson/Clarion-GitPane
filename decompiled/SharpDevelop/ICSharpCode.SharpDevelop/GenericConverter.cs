using System;
using System.ComponentModel;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

internal static class GenericConverter
{
	public static T FromString<T>(string v, T defaultValue)
	{
		if (string.IsNullOrEmpty(v))
		{
			return defaultValue;
		}
		if (typeof(T) == typeof(string))
		{
			return (T)(object)v;
		}
		try
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			return (T)converter.ConvertFromInvariantString(v);
		}
		catch (Exception message)
		{
			LoggingService.Info(message);
			return defaultValue;
		}
	}

	public static string ToString<T>(T val)
	{
		if (typeof(T) == typeof(string))
		{
			string text = (string)(object)val;
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return null;
		}
		try
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			string text2 = converter.ConvertToInvariantString(val);
			return string.IsNullOrEmpty(text2) ? null : text2;
		}
		catch (Exception message)
		{
			LoggingService.Info(message);
			return null;
		}
	}
}
