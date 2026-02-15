using System;
using System.IO;
using System.Reflection;

namespace ICSharpCode.Core;

public static class VersionService
{
	private static IDEVersion ver;

	public static IDEVersion Version
	{
		get
		{
			return ver;
		}
		set
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			if (Path.GetFileName(callingAssembly.Location).Equals("clarion.exe", StringComparison.OrdinalIgnoreCase) || Path.GetFileName(callingAssembly.Location).Equals("clarioncl.exe", StringComparison.OrdinalIgnoreCase) || Path.GetFileName(callingAssembly.Location).Equals("clarioncldev.exe", StringComparison.OrdinalIgnoreCase))
			{
				ver = value;
			}
		}
	}

	static VersionService()
	{
		ver = IDEVersion.Professional;
	}
}
