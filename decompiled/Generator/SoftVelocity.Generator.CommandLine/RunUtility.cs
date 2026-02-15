using System.Collections.Generic;
using Clarion.Core.Redirection;
using Clarion.GEN;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Generator.CommandLine;

internal class RunUtility : GeneratorCL
{
	protected override void DoRun(List<List<string>> parameters, ICommandLineLogger logger, RedirectionFile red)
	{
		Win32Generator.CommandLineLogger = logger;
		foreach (List<string> parameter in parameters)
		{
			Application application = ApplicationService.FetchApplication(parameter[0]);
			if (application != null)
			{
				if (parameter.Count > 2)
				{
					application.GenerateUtility(parameter[1], parameter[2]);
				}
				else
				{
					application.GenerateUtility(parameter[1], null);
				}
			}
		}
	}
}
