using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public static class ColorThemesListService
{
	private static string _themeDirectoryPath = null;

	private static string[] _Themes = new string[0];

	public static string ThemeDirectoryPath
	{
		get
		{
			if (string.IsNullOrEmpty(_themeDirectoryPath))
			{
				Initialize(FileUtility.Combine(PropertyService.DataDirectory, "resources", "ColorThemes"));
			}
			return _themeDirectoryPath;
		}
	}

	public static string[] Items => _Themes;

	public static void Initialize(string themeDirectoryPath)
	{
		if (string.IsNullOrEmpty(_themeDirectoryPath))
		{
			_themeDirectoryPath = themeDirectoryPath;
			if (!Directory.Exists(_themeDirectoryPath))
			{
				Directory.CreateDirectory(_themeDirectoryPath);
			}
		}
	}

	public static void Refresh()
	{
		string[] array = Directory.GetFiles(ThemeDirectoryPath, "*.xml", SearchOption.TopDirectoryOnly);
		if (array.Length > 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Path.GetFileNameWithoutExtension(array[i]);
			}
		}
		else
		{
			CreateDefault();
			array = new string[1] { "Default" };
		}
		_Themes = array;
	}

	private static void CreateDefault()
	{
		AppearanceControl appearanceControl = new AppearanceControl();
		ToolStripProfessionalRenderer toolStripProfessionalRenderer = new ToolStripProfessionalRenderer();
		((CustomColorTable)appearanceControl.Renderer.ColorTable).SetFromProfessionalColorTable(toolStripProfessionalRenderer.ColorTable);
		string xmlFile = Path.Combine(ThemeDirectoryPath, "Default.xml");
		appearanceControl.SaveAppearanceProperties(xmlFile);
	}

	public static string GetThemeFileName(string themeName)
	{
		if (!string.IsNullOrEmpty(themeName))
		{
			return Path.Combine(ThemeDirectoryPath, themeName + ".xml");
		}
		return null;
	}

	public static bool ThemeExist(string themeName)
	{
		string[] items = Items;
		foreach (string text in items)
		{
			if (text.Equals(themeName, StringComparison.OrdinalIgnoreCase))
			{
				if (File.Exists(GetThemeFileName(text)))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public static DialogResult EditTheme(string themeName)
	{
		Refresh();
		if (ThemeExist(themeName))
		{
			string themeFileName = GetThemeFileName(themeName);
			using AppearanceEditor appearanceEditor = new AppearanceEditor(themeFileName);
			DialogResult result = appearanceEditor.ShowDialog(WorkbenchSingleton.MainForm);
			Refresh();
			return result;
		}
		return DialogResult.Abort;
	}

	public static void SetToolStripManagerRendererTheme(string themeName)
	{
		Refresh();
		if (ThemeExist(themeName))
		{
			AppearanceControl appearanceControl = new AppearanceControl(GetThemeFileName(themeName));
			ToolStripManager.Renderer = appearanceControl.Renderer;
		}
	}
}
