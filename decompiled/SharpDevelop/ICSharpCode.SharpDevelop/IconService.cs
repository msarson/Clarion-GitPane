using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public static class IconService
{
	private static Dictionary<string, string> extensionHashtable;

	private static Dictionary<string, string> projectFileHashtable;

	private static readonly char[] separators;

	static IconService()
	{
		extensionHashtable = new Dictionary<string, string>();
		projectFileHashtable = new Dictionary<string, string>();
		separators = new char[2]
		{
			Path.DirectorySeparatorChar,
			Path.VolumeSeparatorChar
		};
		Thread thread = new Thread(LoadThread);
		thread.Name = "IconLoader";
		thread.IsBackground = true;
		thread.Priority = ThreadPriority.Normal;
		thread.Start();
	}

	private static void LoadThread()
	{
		try
		{
			InitializeIcons(AddInTree.GetTreeNode("/Workspace/Icons"));
		}
		catch (TreePathNotFoundException)
		{
		}
	}

	public static Bitmap GetGhostBitmap(string name)
	{
		return GetGhostBitmap(GetBitmap(name));
	}

	public static Bitmap GetGhostBitmap(Bitmap bitmap)
	{
		ColorMatrix newColorMatrix = new ColorMatrix(new float[5][]
		{
			new float[5] { 1f, 0f, 0f, 0f, 0f },
			new float[5] { 0f, 1f, 0f, 0f, 0f },
			new float[5] { 0f, 0f, 1f, 0f, 0f },
			new float[5] { 0f, 0f, 0f, 0.5f, 0f },
			new float[5] { 0f, 0f, 0f, 0f, 1f }
		});
		ImageAttributes imageAttributes = new ImageAttributes();
		imageAttributes.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
		using Graphics graphics = Graphics.FromImage(bitmap2);
		graphics.FillRectangle(SystemBrushes.Window, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
		graphics.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
		return bitmap2;
	}

	public static Bitmap GetBitmap(string name)
	{
		Bitmap bitmap = ResourceService.GetBitmap(name);
		if (bitmap != null)
		{
			return bitmap;
		}
		return ResourceService.GetBitmap("Icons.16x16.MiscFiles");
	}

	public static Icon GetIcon(string name)
	{
		Icon icon = ResourceService.GetIcon(name);
		if (icon != null)
		{
			return icon;
		}
		return ResourceService.GetIcon("Icons.16x16.MiscFiles");
	}

	public static string GetImageForProjectType(string projectType)
	{
		if (projectFileHashtable.ContainsKey(projectType))
		{
			return projectFileHashtable[projectType];
		}
		return "Icons.16x16.SolutionIcon";
	}

	public static string GetImageForFile(string fileName)
	{
		string text = Path.GetExtension(fileName).ToUpperInvariant();
		if (text.Length == 0)
		{
			text = ".TXT";
		}
		if (extensionHashtable.ContainsKey(text))
		{
			return extensionHashtable[text];
		}
		return "Icons.16x16.MiscFiles";
	}

	private static void InitializeIcons(AddInTreeNode treeNode)
	{
		extensionHashtable[".PRJX"] = "Icons.16x16.SolutionIcon";
		extensionHashtable[".CMBX"] = "Icons.16x16.CombineIcon";
		extensionHashtable[".SLN"] = "Icons.16x16.CombineIcon";
		IconDescriptor[] array = (IconDescriptor[])treeNode.BuildChildItems(null).ToArray(typeof(IconDescriptor));
		foreach (IconDescriptor iconDescriptor in array)
		{
			string value = ((iconDescriptor.Resource != null) ? iconDescriptor.Resource : iconDescriptor.Id);
			if (iconDescriptor.Extensions != null)
			{
				string[] extensions = iconDescriptor.Extensions;
				foreach (string text in extensions)
				{
					extensionHashtable[text.ToUpperInvariant()] = value;
				}
			}
			if (iconDescriptor.Language != null)
			{
				projectFileHashtable[iconDescriptor.Language] = value;
			}
		}
	}
}
