using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using ICSharpCode.Core;

namespace Clarion.Core.Options;

public class RedirectionVersion
{
	private Properties props;

	public EventHandler<MacrosChangedEvent> MacrosChanged;

	internal Properties Properties => props;

	public string Name
	{
		get
		{
			return props.Get("Name", "");
		}
		set
		{
			props.Set("Name", value);
		}
	}

	public bool SupportsInclude
	{
		get
		{
			return props.Get("SupportsInclude", defaultValue: true);
		}
		set
		{
			props.Set("SupportsInclude", value);
		}
	}

	public ReadOnlyCollection<KeyValuePair<string, string>> Macros
	{
		get
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			Properties macroProperties = MacroProperties;
			string[] elements = macroProperties.Elements;
			foreach (string text in elements)
			{
				list.Add(new KeyValuePair<string, string>(text, macroProperties[text]));
			}
			string location;
			if (!macroProperties.Contains("reddir"))
			{
				location = Assembly.GetEntryAssembly().Location;
				location = Path.Combine(Path.GetPathRoot(location), Path.GetDirectoryName(location));
				list.Add(new KeyValuePair<string, string>("reddir", location));
			}
			else
			{
				location = macroProperties["reddir"];
			}
			if (!macroProperties.Contains("root"))
			{
				string path = location;
				DirectoryInfo parent = Directory.GetParent(path);
				string[] array = parent.FullName.Split(Path.PathSeparator);
				if (array.Length > 0 && !array[array.Length - 1].Equals("bin", StringComparison.InvariantCultureIgnoreCase))
				{
					parent = parent.Parent;
				}
				list.Add(new KeyValuePair<string, string>("root", parent.FullName));
			}
			return new ReadOnlyCollection<KeyValuePair<string, string>>(list);
		}
		set
		{
			Properties properties = new Properties();
			foreach (KeyValuePair<string, string> item in value)
			{
				properties[item.Key] = item.Value;
			}
			props.Set("Macros", properties);
		}
	}

	public Properties MacroProperties => props.Get("Macros", new Properties());

	internal RedirectionVersion(string name)
	{
		props = new Properties();
		props.Set("Name", name);
		props.Set("SupportsInclude", value: true);
	}

	internal RedirectionVersion(Properties baseProps)
	{
		props = baseProps;
	}

	public void UpdateMacros(Dictionary<string, string> newValues)
	{
		if (MacrosChanged != null)
		{
			MacrosChanged(this, new MacrosChangedEvent(newValues));
		}
		Properties macroProperties = MacroProperties;
		string[] elements = macroProperties.Elements;
		foreach (string text in elements)
		{
			if (!newValues.ContainsKey(text))
			{
				macroProperties.Remove(text);
			}
		}
		foreach (KeyValuePair<string, string> newValue in newValues)
		{
			macroProperties.Set(newValue.Key, newValue.Value);
		}
	}
}
