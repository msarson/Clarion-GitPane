using System.Collections.Generic;
using Clarion.Core.Redirection;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Generator.CommandLine;

internal class UnRegisterTemplate : GeneratorCL
{
	protected override void DoRun(List<List<string>> parameters, ICommandLineLogger logger, RedirectionFile red)
	{
		foreach (List<string> parameter in parameters)
		{
			gen.UnRegisterTemplateChain(parameter[0]);
		}
	}
}
