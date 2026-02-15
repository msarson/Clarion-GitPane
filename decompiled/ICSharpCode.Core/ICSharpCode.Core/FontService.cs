using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public static class FontService
{
	public enum FontType
	{
		Dialogs,
		ListControls,
		TextEditor,
		StartPage
	}

	private const string fontsList = "ListOfFonts";

	private const string appgenFontComponent = "AppGen Dialogs";

	private const string appgenFontDescription = "Application Editor";

	private static SortedDictionary<string, ComponentFont> storedFonts = new SortedDictionary<string, ComponentFont>();

	private static bool changed = false;

	private static bool inited = false;

	private static bool autosave = true;

	public static readonly string ComponentsFontProperty = "CoreProperties.ComponentsFont";

	private static Properties componentsFontProperties;

	public static IEnumerable<ComponentFont> FontComponentsDescriptions
	{
		get
		{
			foreach (ComponentFont f in storedFonts.Values)
			{
				yield return new ComponentFont(f);
			}
		}
	}

	public static bool Changed
	{
		get
		{
			return changed;
		}
		private set
		{
			changed = value;
			if (AutoSave && changed)
			{
				Save();
			}
		}
	}

	public static bool AutoSave
	{
		get
		{
			return autosave;
		}
		set
		{
			autosave = value;
		}
	}

	public static Font ChangeSize(Font originalFont, int deltaSize)
	{
		return new Font(originalFont.FontFamily, originalFont.Size + (float)deltaSize);
	}

	public static Font StringToFont(string font)
	{
		try
		{
			string[] array = font.Split(',');
			return new Font(array[0], float.Parse(array[1]));
		}
		catch (Exception message)
		{
			LoggingService.Warn(message);
			return ResourceService.DefaultMonospacedFont;
		}
	}

	public static string FontToString(Font font)
	{
		return font.Name + "," + (int)font.SizeInPoints;
	}

	public static Font GetFont(FontType fontType)
	{
		return fontType switch
		{
			FontType.Dialogs => GetFont("Dialogs", "Dialogs", SystemInformation.MenuFont), 
			FontType.ListControls => GetFont("ListControls", "List Controls", SystemInformation.MenuFont), 
			FontType.TextEditor => GetFont("TextEditor", "Text Editor, Output Window (Proportional Font)", ResourceService.DefaultMonospacedFont), 
			FontType.StartPage => GetFont("StartPage", "Start Page", new Font(SystemInformation.MenuFont.FontFamily, SystemInformation.MenuFont.SizeInPoints + 8f)), 
			_ => GetFont("Dialogs", "Dialogs", SystemInformation.MenuFont), 
		};
	}

	public static void SetFont(FontType fontType, Font newFont)
	{
		SetFont(fontType, FontToString(newFont));
	}

	public static void SetFont(FontType fontType, string newFont)
	{
		switch (fontType)
		{
		case FontType.Dialogs:
			SetFont("Dialogs", "Dialogs", newFont);
			break;
		case FontType.ListControls:
			SetFont("ListControls", "List Controls", newFont);
			break;
		case FontType.TextEditor:
			SetFont("TextEditor", "Text Editor, Output Window (Proportional Font)", newFont);
			break;
		case FontType.StartPage:
			SetFont("StartPage", "Start Page", newFont);
			break;
		}
	}

	private static void SetFont(string componentName, string componentDescription, string newFont)
	{
		Initialize();
		if (storedFonts.ContainsKey(componentName))
		{
			bool flag = false;
			if (storedFonts[componentName].FontString != newFont)
			{
				storedFonts[componentName].FontString = newFont;
				flag = true;
			}
			Changed = flag;
		}
	}

	private static Font GetFont(string componentName)
	{
		return GetFont(componentName, string.Empty, SystemInformation.MenuFont);
	}

	private static Font GetFont(string componentName, string componentDescription, Font defaultFont)
	{
		Initialize();
		if (!storedFonts.ContainsKey(componentName))
		{
			storedFonts.Add(componentName, new ComponentFont(componentName, componentDescription, defaultFont));
			Changed = true;
		}
		else if (ShouldUpdateDescription(storedFonts[componentName], componentName, componentDescription))
		{
			storedFonts[componentName].Description = componentDescription;
			Changed = true;
		}
		return storedFonts[componentName].Font;
	}

	private static bool ShouldUpdateDescription(ComponentFont cp, string componentName, string componentDescription)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(componentDescription) && storedFonts[componentName].Description != componentDescription && componentDescription != componentName)
		{
			result = true;
		}
		return result;
	}

	public static void Initialize()
	{
		if (!inited)
		{
			componentsFontProperties = PropertyService.Get(ComponentsFontProperty, new Properties());
			inited = true;
			Load();
			InitDefaults();
		}
	}

	private static void InitDefaults()
	{
		GetFont(FontType.Dialogs);
		GetFont(FontType.ListControls);
		GetFont(FontType.TextEditor);
		GetFont(FontType.StartPage);
	}

	public static void Load()
	{
		if (!inited)
		{
			componentsFontProperties = PropertyService.Get(ComponentsFontProperty, new Properties());
			inited = true;
		}
		storedFonts.Clear();
		string[] array = componentsFontProperties.Get("ListOfFonts", new string[0]);
		string[] array2 = array;
		foreach (string componentFont in array2)
		{
			ComponentFont componentFont2 = ComponentFont.FromString(componentFont);
			if (componentFont2.Component != "Dialogs")
			{
				storedFonts.Add(componentFont2.Component, componentFont2);
			}
		}
		LoadExternal();
		Changed = false;
	}

	public static void Save(ComponentFont[] newFontValues)
	{
		bool flag = false;
		AutoSave = false;
		foreach (ComponentFont componentFont in newFontValues)
		{
			if (storedFonts.ContainsKey(componentFont.Component))
			{
				storedFonts[componentFont.Component].Font = componentFont.Font;
				flag = true;
			}
		}
		AutoSave = true;
		Changed = flag;
	}

	public static void Save()
	{
		if (!changed || WinFormsDesigner.IsInDesigner)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (ComponentFont value in storedFonts.Values)
		{
			if (value.Component == "Dialogs")
			{
				SaveAppgenFont(value);
			}
			else
			{
				list.Add(value.ToStringSerialize());
			}
		}
		componentsFontProperties.Set("ListOfFonts", list.ToArray());
		PropertyService.Set(ComponentsFontProperty, componentsFontProperties);
		Changed = false;
	}

	private static void LoadExternal()
	{
		ComponentFont componentFont = LoadAppgenFont();
		componentFont.Component = "Dialogs";
		componentFont.Description = "Dialogs";
		storedFonts.Add(componentFont.Component, componentFont);
	}

	private static void SaveAppgenFont(ComponentFont appFontComponent)
	{
		if (!WinFormsDesigner.IsInDesigner)
		{
			Properties properties = PropertyService.Get("AppGen Dialogs", new Properties());
			properties.Set("DlgFontName", appFontComponent.Font.Name);
			properties.Set("DlgFontSize", (int)appFontComponent.Font.SizeInPoints);
			PropertyService.Set("AppGen Dialogs", properties);
		}
	}

	private static ComponentFont LoadAppgenFont()
	{
		ComponentFont componentFont = new ComponentFont("AppGen Dialogs", "Application Editor", SystemInformation.MenuFont.ToString());
		try
		{
			Properties properties = PropertyService.Get("AppGen Dialogs", new Properties());
			string text = properties.Get("DlgFontName", SystemInformation.MenuFont.Name);
			if (text.StartsWith("\"") && text.EndsWith("\""))
			{
				text = text.Substring(1, text.Length - 2);
			}
			int num = properties.Get("DlgFontSize", (int)SystemInformation.MenuFont.SizeInPoints);
			FontStyle style = FontStyle.Regular;
			componentFont.Font = new Font(text, num, style);
		}
		catch
		{
		}
		return componentFont;
	}
}
