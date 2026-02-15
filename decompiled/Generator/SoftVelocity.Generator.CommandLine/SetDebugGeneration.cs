using System;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Generator.CommandLine;

internal class SetDebugGeneration : ICommandLine
{
	internal static GenerationMode debugMode;

	public bool Enabled => (int)VersionService.Version == 1;

	public void Run(List<List<string>> parameters, ICommandLineLogger logger, object redFile, bool forWindows, string version)
	{
		if (parameters[0][0].Equals("on", StringComparison.InvariantCultureIgnoreCase))
		{
			debugMode = GenerationMode.On;
		}
		else if (parameters[0][0].Equals("off", StringComparison.InvariantCultureIgnoreCase))
		{
			debugMode = GenerationMode.Off;
		}
		else
		{
			logger.Warning("GENW100", ResourceService.GetString("Clarion.Generator.CommandLine.Application.Generate.SetDebugGeneration.BadSwitch"));
		}
	}
}
