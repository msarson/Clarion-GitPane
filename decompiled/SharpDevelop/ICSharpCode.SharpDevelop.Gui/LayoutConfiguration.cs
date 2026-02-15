using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class LayoutConfiguration
{
	private const string DataLayoutSubPath = "resources/layouts";

	private const string configFile = "LayoutConfig.xml";

	public static readonly List<LayoutConfiguration> Layouts = new List<LayoutConfiguration>();

	public static string[] DefaultLayouts = new string[4] { "Application", "Default", "Debug", "Plain" };

	private string name;

	private string fileName;

	private string displayName;

	private bool readOnly;

	private bool custom;

	public bool Custom
	{
		get
		{
			return custom;
		}
		set
		{
			custom = value;
		}
	}

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public string DisplayName
	{
		get
		{
			if (displayName != null)
			{
				return displayName;
			}
			return Name;
		}
		set
		{
			displayName = value;
		}
	}

	public bool ReadOnly
	{
		get
		{
			return readOnly;
		}
		set
		{
			readOnly = value;
		}
	}

	public static string CurrentLayoutName
	{
		get
		{
			return PropertyService.Get("Workbench.CurrentLayout", "Application");
		}
		set
		{
			if (WorkbenchSingleton.InvokeRequired)
			{
				throw new InvalidOperationException("Invoke required");
			}
			if (value != CurrentLayoutName)
			{
				PropertyService.Set("Workbench.CurrentLayout", value);
				WorkbenchSingleton.Workbench.WorkbenchLayout.LoadConfiguration();
				OnLayoutChanged(EventArgs.Empty);
			}
		}
	}

	public static string CurrentLayoutFileName
	{
		get
		{
			string path = Path.Combine(PropertyService.ConfigDirectory, "layouts");
			LayoutConfiguration currentLayout = CurrentLayout;
			if (currentLayout != null)
			{
				return Path.Combine(path, currentLayout.FileName);
			}
			return null;
		}
	}

	public static string CurrentLayoutTemplateFileName
	{
		get
		{
			string path = Path.Combine(PropertyService.DataDirectory, "resources/layouts");
			LayoutConfiguration currentLayout = CurrentLayout;
			if (currentLayout != null)
			{
				return Path.Combine(path, currentLayout.FileName);
			}
			return null;
		}
	}

	public static LayoutConfiguration CurrentLayout
	{
		get
		{
			foreach (LayoutConfiguration layout in Layouts)
			{
				if (layout.name == CurrentLayoutName)
				{
					return layout;
				}
			}
			return null;
		}
	}

	public static event EventHandler LayoutChanged;

	private LayoutConfiguration()
	{
	}

	private LayoutConfiguration(XmlElement el, bool custom)
	{
		name = el.GetAttribute("name");
		fileName = el.GetAttribute("file");
		readOnly = bool.Parse(el.GetAttribute("readonly"));
		this.custom = custom;
	}

	public static LayoutConfiguration CreateCustom(string name)
	{
		LayoutConfiguration layoutConfiguration = new LayoutConfiguration();
		layoutConfiguration.name = name;
		layoutConfiguration.fileName = Path.GetRandomFileName() + ".xml";
		File.Copy(Path.Combine(Path.Combine(PropertyService.DataDirectory, "resources/layouts"), "Application.xml"), Path.Combine(Path.Combine(PropertyService.ConfigDirectory, "layouts"), layoutConfiguration.fileName));
		layoutConfiguration.custom = true;
		Layouts.Add(layoutConfiguration);
		return layoutConfiguration;
	}

	public override string ToString()
	{
		return DisplayName;
	}

	public static LayoutConfiguration GetLayout(string name)
	{
		foreach (LayoutConfiguration layout in Layouts)
		{
			if (layout.Name == name)
			{
				return layout;
			}
		}
		return null;
	}

	internal static void LoadLayoutConfiguration()
	{
		Layouts.Clear();
		string path = Path.Combine(PropertyService.ConfigDirectory, "layouts");
		if (File.Exists(Path.Combine(path, "LayoutConfig.xml")))
		{
			LoadLayoutConfiguration(Path.Combine(path, "LayoutConfig.xml"), custom: true);
		}
		string path2 = Path.Combine(PropertyService.DataDirectory, "resources/layouts");
		if (File.Exists(Path.Combine(path2, "LayoutConfig.xml")))
		{
			LoadLayoutConfiguration(Path.Combine(path2, "LayoutConfig.xml"), custom: false);
		}
	}

	private static void LoadLayoutConfiguration(string layoutConfig, bool custom)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(layoutConfig);
		foreach (XmlElement childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			Layouts.Add(new LayoutConfiguration(childNode, custom));
		}
	}

	public static void SaveCustomLayoutConfiguration()
	{
		string path = Path.Combine(PropertyService.ConfigDirectory, "layouts");
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(Path.Combine(path, "LayoutConfig.xml"), Encoding.UTF8);
		xmlTextWriter.Formatting = Formatting.Indented;
		xmlTextWriter.WriteStartElement("LayoutConfig");
		foreach (LayoutConfiguration layout in Layouts)
		{
			if (layout.custom)
			{
				xmlTextWriter.WriteStartElement("Layout");
				xmlTextWriter.WriteAttributeString("name", layout.name);
				xmlTextWriter.WriteAttributeString("file", layout.fileName);
				xmlTextWriter.WriteAttributeString("readonly", layout.readOnly.ToString());
				xmlTextWriter.WriteEndElement();
			}
		}
		xmlTextWriter.WriteEndElement();
	}

	protected static void OnLayoutChanged(EventArgs e)
	{
		if (LayoutConfiguration.LayoutChanged != null)
		{
			LayoutConfiguration.LayoutChanged(null, e);
		}
	}
}
