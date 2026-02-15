using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Clarion.Core.Resources;

namespace Clarion.Core;

public class EnumHolder<T>
{
	private string text;

	private static Dictionary<T, EnumHolder<T>> enums = new Dictionary<T, EnumHolder<T>>();

	public readonly T Value;

	private EnumHolder(T enumValue)
	{
		Value = enumValue;
	}

	public static EnumHolder<T> GetEnumHolder(T enumValue)
	{
		if (!enums.TryGetValue(enumValue, out var value))
		{
			value = new EnumHolder<T>(enumValue);
			enums.Add(enumValue, value);
		}
		return value;
	}

	public override string ToString()
	{
		if (text == null)
		{
			MemberInfo[] member = Value.GetType().GetMember(Value.ToString());
			if (member != null && member.Length > 0)
			{
				object[] customAttributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
				if (customAttributes != null && customAttributes.Length > 0)
				{
					text = IntenalResources.GetString(((DescriptionAttribute)customAttributes[0]).Description);
				}
			}
			else
			{
				text = Value.ToString();
			}
		}
		return text;
	}
}
