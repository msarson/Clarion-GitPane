using System.Collections.Generic;
using Clarion.Core.Redirection;
using Clarion.GEN;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Generator.CommandLine;

internal class Import : GeneratorCL
{
	protected override void DoRun(List<List<string>> parameters, ICommandLineLogger logger, RedirectionFile red)
	{
		foreach (List<string> parameter in parameters)
		{
			Win32App win32App = ApplicationService.NewAppFromTxa(parameter[0], parameter[1]);
			if (win32App == null)
			{
				logger.Error("GENE003", "Error importing txa");
			}
			else
			{
				win32App.Close();
			}
		}
	}
}
