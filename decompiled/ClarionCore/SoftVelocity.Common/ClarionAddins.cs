using ICSharpCode.Core;

namespace SoftVelocity.Common;

public class ClarionAddins
{
	private static bool win32 = false;

	private static bool dotNet = false;

	private static bool searched = false;

	private static bool defaultProjectWindow_read = false;

	private static bool defaultProjectWindow = true;

	public static bool IsDefaultProjectWin
	{
		get
		{
			if (DotNetPresent && Win32Present)
			{
				if (!defaultProjectWindow_read)
				{
					defaultProjectWindow_read = true;
					defaultProjectWindow = PropertyService.Get("IsDefaultProjectWin", defaultProjectWindow, "ProjectsAndSolution");
				}
				return defaultProjectWindow;
			}
			return Win32Present;
		}
	}

	public static bool DotNetPresent
	{
		get
		{
			Init();
			return dotNet;
		}
	}

	public static bool Win32Present
	{
		get
		{
			Init();
			return win32;
		}
	}

	private static void Init()
	{
		if (searched)
		{
			return;
		}
		foreach (AddIn addIn in AddInTree.AddIns)
		{
			if (addIn.Name.Equals("ClarionWindowsBinding"))
			{
				win32 = true;
			}
			else if (addIn.Name.Equals("ClarionNetBinding"))
			{
				dotNet = true;
			}
			if (win32 && dotNet)
			{
				break;
			}
		}
		searched = true;
	}

	public static void ResetIsDefaultProjectWin()
	{
		defaultProjectWindow_read = false;
		_ = IsDefaultProjectWin;
	}
}
