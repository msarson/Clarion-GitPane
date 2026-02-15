using System.Reflection;
using ICSharpCode.Core;

namespace Clarion.Core.Resources;

internal class IntenalResources
{
	private static bool loaded;

	private static void Load()
	{
		if (!loaded)
		{
			ResourceService.RegisterStrings("Clarion.Core.Resources.Clarion.Core", Assembly.GetExecutingAssembly());
			loaded = true;
		}
	}

	internal static string GetString(string s)
	{
		Load();
		return ResourceService.GetString(s);
	}
}
