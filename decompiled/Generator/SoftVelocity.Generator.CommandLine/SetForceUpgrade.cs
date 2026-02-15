using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using SoftVelocity.DataDictionary;

namespace SoftVelocity.Generator.CommandLine;

internal class SetForceUpgrade : ICommandLine
{
	internal static bool forceUpgrade;

	public bool Enabled => (int)VersionService.Version == 1;

	public void Run(List<List<string>> parameters, ICommandLineLogger logger, object redFile, bool forWindows, string version)
	{
		forceUpgrade = true;
		DataDictionaryService.AskOnConvert = false;
	}
}
