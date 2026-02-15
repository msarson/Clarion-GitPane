using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace ICSharpCode.Core;

public static class StringParser
{
	private static readonly Dictionary<string, string> properties;

	private static readonly Dictionary<string, IStringTagProvider> stringTagProviders;

	private static readonly Dictionary<string, object> propertyObjects;

	public static Dictionary<string, string> Properties => properties;

	public static Dictionary<string, object> PropertyObjects => propertyObjects;

	static StringParser()
	{
		properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		stringTagProviders = new Dictionary<string, IStringTagProvider>(StringComparer.InvariantCultureIgnoreCase);
		propertyObjects = new Dictionary<string, object>();
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		if (entryAssembly != null)
		{
			string location = entryAssembly.Location;
			propertyObjects["exe"] = FileVersionInfo.GetVersionInfo(location);
		}
		properties["USER"] = Environment.UserName;
		properties["Version"] = "10.0.12463";
		if (IntPtr.Size == 4)
		{
			properties["Platform"] = "Win32";
		}
		else if (IntPtr.Size == 8)
		{
			properties["Platform"] = "Win64";
		}
		else
		{
			properties["Platform"] = "unknown";
		}
	}

	public static string Format(string input, params object[] args)
	{
		return string.Format(Parse(input), args);
	}

	public static string Parse(string input)
	{
		return Parse(input, null);
	}

	public static void Parse(string[] inputs)
	{
		for (int i = 0; i < inputs.Length; i++)
		{
			inputs[i] = Parse(inputs[i], null);
		}
	}

	public static void RegisterStringTagProvider(IStringTagProvider tagProvider)
	{
		string[] tags = tagProvider.Tags;
		foreach (string key in tags)
		{
			stringTagProviders[key] = tagProvider;
		}
	}

	public static string Parse(string input, string[,] customTags)
	{
		return Parse(input, customTags, null, null, null);
	}

	public static string Parse(string input, string primaryTagsProviderPrefix, IStringTagProvider primaryTagsProvider)
	{
		return Parse(input, null, primaryTagsProviderPrefix, primaryTagsProvider, null);
	}

	public static string Parse(string input, string[,] customTags, string primaryTagsProviderPrefix, IStringTagProvider primaryTagsProvider)
	{
		return Parse(input, customTags, primaryTagsProviderPrefix, primaryTagsProvider, null);
	}

	public static string Parse(string input, string[,] customTags, string primaryTagsProviderPrefix, IStringTagProvider primaryTagsProvider, Dictionary<string, IStringTagProvider> customTagsProviders)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		int num = 0;
		StringBuilder stringBuilder = null;
		do
		{
			int num2 = num;
			num = input.IndexOf("${", num);
			if (num < 0)
			{
				if (stringBuilder == null)
				{
					return input;
				}
				if (num2 < input.Length)
				{
					stringBuilder.Append(input, num2, input.Length - num2);
				}
				return stringBuilder.ToString();
			}
			if (stringBuilder == null)
			{
				stringBuilder = ((num != 0) ? new StringBuilder(input, 0, num, num + 16) : new StringBuilder());
			}
			else if (num > num2)
			{
				stringBuilder.Append(input, num2, num - num2);
			}
			int num3 = input.IndexOf('}', num + 1);
			if (num3 < 0)
			{
				stringBuilder.Append("${");
				num += 2;
				continue;
			}
			string text = input.Substring(num + 2, num3 - num - 2);
			string value = GetValue(text, customTags, primaryTagsProviderPrefix, primaryTagsProvider, customTagsProviders);
			if (value == null)
			{
				stringBuilder.Append("${");
				stringBuilder.Append(text);
				stringBuilder.Append('}');
			}
			else
			{
				stringBuilder.Append(value);
			}
			num = num3 + 1;
		}
		while (num < input.Length);
		return stringBuilder.ToString();
	}

	private static string GetValue(string propertyName, string[,] customTags, string primaryTagsProviderPrefix, IStringTagProvider primaryTagsProvider, Dictionary<string, IStringTagProvider> customTagsProviders)
	{
		if (propertyName.StartsWith("res:"))
		{
			try
			{
				return Parse(ResourceService.GetString(propertyName.Substring(4)), customTags);
			}
			catch (ResourceNotFoundException)
			{
				return null;
			}
		}
		if (propertyName.Equals("DATE", StringComparison.OrdinalIgnoreCase))
		{
			return DateTime.Today.ToShortDateString();
		}
		if (propertyName.Equals("TIME", StringComparison.OrdinalIgnoreCase))
		{
			return DateTime.Now.ToShortTimeString();
		}
		if (propertyName.Equals("ProductName", StringComparison.OrdinalIgnoreCase))
		{
			return MessageService.ProductName;
		}
		if (propertyName.Equals("GUID", StringComparison.OrdinalIgnoreCase))
		{
			return Guid.NewGuid().ToString().ToUpperInvariant();
		}
		if (customTags != null)
		{
			for (int i = 0; i < customTags.GetLength(0); i++)
			{
				if (propertyName.Equals(customTags[i, 0], StringComparison.OrdinalIgnoreCase))
				{
					return customTags[i, 1];
				}
			}
		}
		if (primaryTagsProviderPrefix != null && propertyName.StartsWith(primaryTagsProviderPrefix))
		{
			return Parse(primaryTagsProvider.Convert(propertyName.Substring(primaryTagsProviderPrefix.Length)), customTags, primaryTagsProviderPrefix, primaryTagsProvider, customTagsProviders);
		}
		if (customTagsProviders != null)
		{
			foreach (string key in customTagsProviders.Keys)
			{
				if (propertyName.StartsWith(key))
				{
					try
					{
						IStringTagProvider stringTagProvider = customTagsProviders[key];
						return Parse(stringTagProvider.Convert(propertyName.Substring(key.Length)), customTags, primaryTagsProviderPrefix, primaryTagsProvider, customTagsProviders);
					}
					catch (ResourceNotFoundException)
					{
						return null;
					}
				}
			}
		}
		if (properties.ContainsKey(propertyName))
		{
			return properties[propertyName];
		}
		if (stringTagProviders.ContainsKey(propertyName))
		{
			return stringTagProviders[propertyName].Convert(propertyName);
		}
		int num = propertyName.IndexOf(':');
		if (num <= 0)
		{
			return null;
		}
		string text = propertyName.Substring(0, num);
		propertyName = propertyName.Substring(num + 1);
		switch (text.ToUpperInvariant())
		{
		case "ADDINPATH":
			foreach (AddIn addIn in AddInTree.AddIns)
			{
				if (addIn.Manifest.Identities.ContainsKey(propertyName))
				{
					return Path.GetDirectoryName(addIn.FileName);
				}
			}
			return null;
		case "ENV":
			return Environment.GetEnvironmentVariable(propertyName);
		case "RES":
			try
			{
				return Parse(ResourceService.GetString(propertyName), customTags);
			}
			catch (ResourceNotFoundException)
			{
				return null;
			}
		case "PROPERTY":
			return GetProperty(propertyName);
		default:
			if (propertyObjects.ContainsKey(text))
			{
				return Get(propertyObjects[text], propertyName);
			}
			return null;
		}
	}

	private static string GetProperty(string propertyName)
	{
		string defaultValue = "";
		int num = propertyName.LastIndexOf("??");
		if (num >= 0)
		{
			defaultValue = propertyName.Substring(num + 2);
			propertyName = propertyName.Substring(0, num);
		}
		num = propertyName.IndexOf('/');
		if (num >= 0)
		{
			Properties properties = PropertyService.Get(propertyName.Substring(0, num), new Properties());
			propertyName = propertyName.Substring(num + 1);
			num = propertyName.IndexOf('/');
			while (num >= 0)
			{
				properties = properties.Get(propertyName.Substring(0, num), new Properties());
				propertyName = propertyName.Substring(num + 1);
			}
			return properties.Get(propertyName, defaultValue);
		}
		return PropertyService.Get(propertyName, defaultValue);
	}

	private static string Get(object obj, string name)
	{
		Type type = obj.GetType();
		PropertyInfo property = type.GetProperty(name);
		if (property != null)
		{
			return property.GetValue(obj, null).ToString();
		}
		FieldInfo field = type.GetField(name);
		if (field != null)
		{
			return field.GetValue(obj).ToString();
		}
		return null;
	}
}
