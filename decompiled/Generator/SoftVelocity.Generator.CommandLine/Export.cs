using System.Collections.Generic;
using Clarion.Core.Redirection;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Generator.CommandLine;

internal class Export : GeneratorCL
{
	protected override void DoRun(List<List<string>> parameters, ICommandLineLogger logger, RedirectionFile red)
	{
		foreach (List<string> parameter in parameters)
		{
			Application application = ApplicationService.FetchApplication(parameter[0]);
			if (application == null)
			{
				logger.Error("GENE003", "Error exporting to txa");
				continue;
			}
			application.QuietConvert = SetForceUpgrade.forceUpgrade;
			if (!application.ExportAll(parameter[1]))
			{
				logger.Error("GENE004", "Error exporting to txa");
			}
			application.Close(forceClose: true);
		}
	}
}
