using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

internal class IconManager
{
	private static ImageList icons;

	private static Hashtable iconIndecies;

	public static ImageList List => icons;

	static IconManager()
	{
		icons = new ImageList();
		iconIndecies = new Hashtable();
		icons.ColorDepth = ColorDepth.Depth32Bit;
	}

	public static int GetIndexForFile(string file)
	{
		string key = ((!Path.GetExtension(file).Equals(".ico", StringComparison.OrdinalIgnoreCase) && !Path.GetExtension(file).Equals(".exe", StringComparison.OrdinalIgnoreCase)) ? Path.GetExtension(file).ToLower() : file);
		if (icons.Images.Count > 100)
		{
			icons.Images.Clear();
			iconIndecies.Clear();
		}
		if (iconIndecies.Contains(key))
		{
			return (int)iconIndecies[key];
		}
		icons.Images.Add(DriveObject.GetImageForFile(file));
		int num = icons.Images.Count - 1;
		iconIndecies.Add(key, num);
		return num;
	}
}
