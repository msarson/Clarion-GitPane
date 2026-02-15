using System.Collections.Generic;
using Clarion.Core.Redirection;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Generator.CommandLine;

internal class ListTemplates : GeneratorCL
{
	protected override void DoRun(List<List<string>> parameters, ICommandLineLogger logger, RedirectionFile red)
	{
		string[] registeredTemplates = gen.RegisteredTemplates;
		foreach (string text in registeredTemplates)
		{
			logger.Message(text);
		}
	}
}
