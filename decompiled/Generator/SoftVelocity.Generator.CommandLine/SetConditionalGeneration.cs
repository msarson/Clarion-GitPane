using System;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Generator.CommandLine;

internal class SetConditionalGeneration : ICommandLine
{
	internal static GenerationMode generationMode;

	public bool Enabled => (int)VersionService.Version == 1;

	public void Run(List<List<string>> parameters, ICommandLineLogger logger, object redFile, bool forWindows, string version)
	{
		if (parameters[0][0].Equals("on", StringComparison.InvariantCultureIgnoreCase))
		{
			generationMode = GenerationMode.On;
		}
		else if (parameters[0][0].Equals("off", StringComparison.InvariantCultureIgnoreCase))
		{
			generationMode = GenerationMode.Off;
		}
		else
		{
			logger.Warning("GENW100", ResourceService.GetString("Clarion.Generator.CommandLine.Application.Generate.SetConditionalGeneration.BadSwitch"));
		}
	}
}
