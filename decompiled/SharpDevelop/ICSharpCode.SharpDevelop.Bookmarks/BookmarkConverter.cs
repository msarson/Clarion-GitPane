using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using ICSharpCode.SharpDevelop.Debugging;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public sealed class BookmarkConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string[] array = ((string)value).Split('|');
			string fileName = array[1];
			int num = int.Parse(array[2], culture);
			if (num < 0)
			{
				return null;
			}
			string text;
			SDBookmark sDBookmark = (((text = array[0]) == null || !(text == "Breakpoint")) ? new SDBookmark(fileName, null, num) : new BreakpointBookmark(fileName, null, num));
			sDBookmark.IsEnabled = bool.Parse(array[3]);
			return sDBookmark;
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		SDBookmark sDBookmark = value as SDBookmark;
		if (destinationType == typeof(string) && sDBookmark != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (sDBookmark is BreakpointBookmark)
			{
				stringBuilder.Append("Breakpoint");
			}
			else
			{
				stringBuilder.Append("Bookmark");
			}
			stringBuilder.Append('|');
			stringBuilder.Append(sDBookmark.FileName);
			stringBuilder.Append('|');
			stringBuilder.Append(sDBookmark.LineNumber);
			stringBuilder.Append('|');
			stringBuilder.Append(sDBookmark.IsEnabled.ToString());
			return stringBuilder.ToString();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
