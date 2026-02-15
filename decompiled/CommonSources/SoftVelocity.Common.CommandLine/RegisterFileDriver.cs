using System.Collections.Generic;
using Clarion.ASL;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Common.CommandLine;

public class RegisterFileDriver : ICommandLine
{
	public bool Enabled => true;

	public void Run(List<List<string>> parameters, ICommandLineLogger logger, object redFile, bool forWindows, string version)
	{
		foreach (List<string> parameter in parameters)
		{
			string empty = string.Empty;
			if (FileDriverRegistry.RegisterFileDriver(parameter[0], ref empty) == null)
			{
				logger.Error("DRVE001", empty);
			}
		}
		PropertyService.Save();
	}
}
